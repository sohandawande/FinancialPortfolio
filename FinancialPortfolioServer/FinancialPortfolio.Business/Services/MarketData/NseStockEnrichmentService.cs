using FinancialPortfolio.Business.Abstractions.IFundamentals;
using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Abstractions.IMarketData;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Data.Entities.Stock;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Response.MarketData;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FinancialPortfolio.Business.Services.MarketData
{
    public sealed class NseStockEnrichmentService : INseStockEnrichmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly INseBhavcopyDownloader _downloader;
        private readonly IFundamentalDataProvider _fundamentalProvider;
        private readonly IApplicationLoggerService _logger;
        private readonly FundamentalDataSettings _settings;

        public NseStockEnrichmentService(
            ApplicationDbContext context,
            INseBhavcopyDownloader downloader,
            IFundamentalDataProvider fundamentalProvider,
            IApplicationLoggerService logger,
            IOptions<FundamentalDataSettings> options)
        {
            _context = Guard.AgainstNull(context, nameof(context));
            _downloader = Guard.AgainstNull(downloader, nameof(downloader));
            _fundamentalProvider = Guard.AgainstNull(fundamentalProvider, nameof(fundamentalProvider));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _settings = Guard.AgainstNull(options.Value, nameof(options));
        }

        public async Task<MarketDataSyncResponse> EnrichAsync(DateOnly tradeDate, CancellationToken cancellationToken = default)
        {
            var weekTask = _downloader.DownloadWeek52Async(tradeDate, cancellationToken);
            var indexTask = _downloader.DownloadIndexMembershipAsync(cancellationToken);
            var stocks = await _context.Stocks
                .Include(x => x.StockDetail)
                .Where(x => x.StockDetail != null)
                .ToListAsync(cancellationToken);

            // Keep the enrichment workload configurable. The first N stocks are processed,
            // while the rest still receive the normal market-data/index processing below.
            var symbols = stocks
                .Select(x => x.Symbol.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(GetMaxStocks())
                .ToArray();

            var fundamentalTask = _fundamentalProvider.GetAsync(symbols, tradeDate, cancellationToken);
            await Task.WhenAll(weekTask, indexTask, fundamentalTask);

            var weekBySymbol = ToMap(await weekTask, x => x.Symbol, x => x);
            var indexBySymbol = ToMap(await indexTask, x => x.Symbol, x => x);
            var fundamentalBySymbol = ToMap(await fundamentalTask, x => x.Symbol, x => x);
            if (fundamentalBySymbol.Count == 0)
                await _logger.LogWarningAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    $"NSE fundamental reports returned no data for {tradeDate:yyyy-MM-dd}. Prices remain available.",
                    cancellationToken);

            var result = new MarketDataSyncResponse { FundamentalRecords = fundamentalBySymbol.Count };
            foreach (var stock in stocks)
            {
                var detail = stock.StockDetail;
                if (detail is null) continue;
                var symbol = stock.Symbol.Trim();

                if (fundamentalBySymbol.TryGetValue(symbol, out var fundamental))
                {
                    if (fundamental.MarketCapCrore is > 0) { detail.MarketCap = fundamental.MarketCapCrore.Value; result.McapUpdated++; }
                    if (fundamental.PE is > 0) { detail.PE = fundamental.PE.Value; result.PeUpdated++; }
                    if (fundamental.EPS is > 0) { detail.EPS = fundamental.EPS.Value; result.EpsUpdated++; }
                    else if (fundamental.PE is > 0 && detail.CurrentPrice > 0) { detail.EPS = Math.Round(detail.CurrentPrice / fundamental.PE.Value, 4); result.EpsUpdated++; }
                }

                if (weekBySymbol.TryGetValue(symbol, out var week))
                {
                    if (week.High > 0) detail.Week52High = week.High;
                    if (week.Low > 0) detail.Week52Low = week.Low;
                    result.Week52Updated++;
                }

                if (indexBySymbol.TryGetValue(symbol, out var member) && !string.IsNullOrWhiteSpace(member.Industry)
                    && (string.IsNullOrWhiteSpace(stock.Industry) || string.Equals(stock.Industry, "Equity", StringComparison.OrdinalIgnoreCase)))
                {
                    stock.Industry = NseCsvHelper.Truncate(member.Industry, 50);
                    result.IndustryUpdated++;
                }
            }

            result.CapClassified = Classify(stocks, indexBySymbol);
            await _context.SaveChangesAsync(cancellationToken);
            return result;
        }

        private int GetMaxStocks()
            => Math.Max(1, _settings.MaxStocks);

        private static int Classify(List<StockEntity> stocks, IReadOnlyDictionary<string, NseIndexMemberRow> indexBySymbol)
        {
            var classified = 0;
            foreach (var stock in stocks)
            {
                var detail = stock.StockDetail;
                if (detail is null) continue;
                var label = MarketCapCategoryHelper.FromSeries(stock.Series);
                if (string.IsNullOrEmpty(label) && indexBySymbol.TryGetValue(stock.Symbol.Trim(), out var member)) label = member.Category;
                if (!string.IsNullOrEmpty(label) && !string.Equals(detail.Category, label, StringComparison.Ordinal)) { detail.Category = label; classified++; }
            }

            var ranked = stocks.Where(s => s.StockDetail is not null && s.StockDetail.MarketCap > 0 && !MarketCapCategoryHelper.IsSmeSeries(s.Series) && MarketCapCategoryHelper.IsPlaceholder(s.StockDetail.Category)).OrderByDescending(s => s.StockDetail!.MarketCap).ToList();
            for (var i = 0; i < ranked.Count; i++)
            {
                var label = MarketCapCategoryHelper.FromRank(i + 1);
                if (string.IsNullOrEmpty(label)) continue;
                var detail = ranked[i].StockDetail!;
                if (!string.Equals(detail.Category, label, StringComparison.Ordinal)) { detail.Category = label; classified++; }
            }
            foreach (var stock in stocks)
                if (stock.StockDetail is not null && MarketCapCategoryHelper.IsPlaceholder(stock.StockDetail.Category)) stock.StockDetail.Category = MarketCapCategoryHelper.Others;
            return classified;
        }

        private static Dictionary<string, TValue> ToMap<T, TValue>(IReadOnlyList<T> rows, Func<T, string> key, Func<T, TValue> value)
            => rows.Where(x => !string.IsNullOrWhiteSpace(key(x))).GroupBy(x => key(x).Trim(), StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => value(g.First()), StringComparer.OrdinalIgnoreCase);
    }
}

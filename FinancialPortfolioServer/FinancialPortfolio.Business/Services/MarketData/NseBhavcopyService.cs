using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Abstractions.IMarketData;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Data.Entities.Stock;
using FinancialPortfolio.Models.Abstractions.ICurrentUser;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Exceptions;
using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Request.MarketData;
using FinancialPortfolio.Models.Model.Response.MarketData;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FinancialPortfolio.Business.Services.MarketData
{
    public sealed class NseBhavcopyService : INseBhavcopyService
    {
        private readonly ApplicationDbContext _context;
        private readonly INseBhavcopyDownloader _downloader;
        private readonly INseStockEnrichmentService _enrichment;
        private readonly IApplicationLoggerService _logger;
        private readonly TimeProvider _timeProvider;
        private readonly ICurrentUserService _currentUser;
        private readonly FundamentalDataSettings _settings;

        public NseBhavcopyService(
            ApplicationDbContext context,
            INseBhavcopyDownloader downloader,
            INseStockEnrichmentService enrichment,
            IApplicationLoggerService logger,
            TimeProvider timeProvider,
            ICurrentUserService currentUser,
            IOptions<FundamentalDataSettings> options)
        {
            _context = Guard.AgainstNull(context, nameof(context));
            _downloader = Guard.AgainstNull(downloader, nameof(downloader));
            _enrichment = Guard.AgainstNull(enrichment, nameof(enrichment));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _timeProvider = Guard.AgainstNull(timeProvider, nameof(timeProvider));
            _currentUser = Guard.AgainstNull(currentUser, nameof(currentUser));
            _settings = Guard.AgainstNull(options.Value, nameof(options));
        }

        public async Task<ApiResponse<MarketDataSyncResponse>> SyncAsync(
            MarketDataSyncRequest request,
            CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(request, nameof(request));
            await AttachSystemAuditUserAsync(cancellationToken);

            var download = await _downloader.DownloadAsync(request.TradeDate, cancellationToken);
            if (download is null)
            {
                await _logger.LogWarningAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    "NSE equity bhavcopy was not available for the requested window.",
                    cancellationToken);
                throw new NotFoundException("Equity bhavcopy is not available yet. Try after market close.");
            }

            var (tradeDate, downloadedRows) = download.Value;
            var maxStocks = GetMaxStocks();
            var nseRows = downloadedRows
                .Where(x => !string.IsNullOrWhiteSpace(x.Symbol))
                .Take(maxStocks)
                .ToList();
            var now = _timeProvider.GetUtcNow().UtcDateTime;

            var inserted = 0;
            var updated = 0;
            var skipped = 0;

            try
            {
                var existing = await _context.Stocks
                    .Include(x => x.StockDetail)
                    .ToListAsync(cancellationToken);

                var bySymbol = existing.ToDictionary(
                    x => x.Symbol.Trim().ToUpperInvariant(),
                    StringComparer.OrdinalIgnoreCase);

                var byIsin = existing
                    .Where(x => !string.IsNullOrWhiteSpace(x.ISINCode))
                    .GroupBy(x => x.ISINCode.Trim().ToUpperInvariant(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                foreach (var row in nseRows)
                {
                    var symbol = row.Symbol.Trim().ToUpperInvariant();
                    var isin = row.Isin?.Trim().ToUpperInvariant();

                    if (bySymbol.TryGetValue(symbol, out var stock)
                        || (!string.IsNullOrWhiteSpace(isin) && byIsin.TryGetValue(isin, out stock)))
                    {
                        UpdateStock(stock, row, now);
                        updated++;
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(isin) && byIsin.ContainsKey(isin))
                    {
                        skipped++;
                        continue;
                    }

                    var created = CreateStock(row, now);
                    bySymbol[symbol] = created;
                    if (!string.IsNullOrWhiteSpace(created.ISINCode))
                        byIsin[created.ISINCode.Trim().ToUpperInvariant()] = created;
                    inserted++;
                }

                await _context.SaveChangesAsync(cancellationToken);

                var enrichment = await _enrichment.EnrichAsync(tradeDate, cancellationToken);

                var response = new MarketDataSyncResponse
                {
                    Source = "NSE",
                    TradeDate = tradeDate,
                    TotalRecords = nseRows.Count,
                    InsertedRecords = inserted,
                    UpdatedRecords = updated,
                    SkippedRecords = skipped,
                    NseRecords = nseRows.Count,
                    FundamentalRecords = enrichment.FundamentalRecords,
                    McapUpdated = enrichment.McapUpdated,
                    CapClassified = enrichment.CapClassified,
                    PeUpdated = enrichment.PeUpdated,
                    EpsUpdated = enrichment.EpsUpdated,
                    Week52Updated = enrichment.Week52Updated,
                    IndustryUpdated = enrichment.IndustryUpdated
                };

                await _logger.LogInformationAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    $"NSE EQ/SME sync for {tradeDate:yyyy-MM-dd}. Downloaded={downloadedRows.Count}, Limited={nseRows.Count}, MaxStocks={maxStocks}, Inserted={inserted}, Updated={updated}, Skipped={skipped}, Fundamentals={response.FundamentalRecords}, Mcap={response.McapUpdated}, PE={response.PeUpdated}, EPS={response.EpsUpdated}",
                    cancellationToken);

                return ResponseFactory.Success(
                    response,
                    $"NSE prices and fundamentals updated for {tradeDate:dd MMM yyyy}. Stocks processed {nseRows.Count}/{downloadedRows.Count}. MarketCap {response.McapUpdated}, PE {response.PeUpdated}, EPS {response.EpsUpdated}.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    ex.Message,
                    ex,
                    cancellationToken);
                throw;
            }
        }

        private int GetMaxStocks()
            => Math.Max(1, _settings.MaxStocks);

        private async Task AttachSystemAuditUserAsync(CancellationToken cancellationToken)
        {
            if (_currentUser.UserId > 0)
                return;

            var admin = await _context.AppUsers
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.Id)
                .Select(x => new { x.Id, x.IdentityUserId })
                .FirstOrDefaultAsync(cancellationToken);

            if (admin is null)
                return;

            _currentUser.SetUnauthenticatedUserContext(admin.Id, admin.IdentityUserId, "nse-sync@system");
        }

        private StockEntity CreateStock(NseBhavcopyRow row, DateTime now)
        {
            var category = MarketCapCategoryHelper.FromSeries(row.Series);
            if (string.IsNullOrEmpty(category))
                category = "NSE";

            var stock = new StockEntity
            {
                Symbol = row.Symbol,
                CompanyName = string.IsNullOrWhiteSpace(row.CompanyName) ? row.Symbol : row.CompanyName,
                Industry = "Equity",
                ISINCode = string.IsNullOrWhiteSpace(row.Isin) ? $"NSE-{row.Symbol}" : row.Isin,
                Series = row.Series,
                StockDetail = ApplyPrices(new StockDetailEntity
                {
                    Category = category,
                    IsActive = true,
                    LogoUrl = $"/logos/{row.Symbol}.png"
                }, row, now)
            };

            _context.Stocks.Add(stock);
            return stock;
        }

        private static void UpdateStock(StockEntity stock, NseBhavcopyRow row, DateTime now)
        {
            if (!string.IsNullOrWhiteSpace(row.CompanyName))
                stock.CompanyName = row.CompanyName;

            if (!string.IsNullOrWhiteSpace(row.Series))
                stock.Series = row.Series;

            if (string.IsNullOrWhiteSpace(stock.ISINCode) && !string.IsNullOrWhiteSpace(row.Isin))
                stock.ISINCode = row.Isin;

            stock.StockDetail ??= new StockDetailEntity
            {
                Category = MarketCapCategoryHelper.FromSeries(row.Series),
                IsActive = true,
                LogoUrl = $"/logos/{stock.Symbol}.png"
            };

            if (string.IsNullOrWhiteSpace(stock.StockDetail.LogoUrl))
                stock.StockDetail.LogoUrl = $"/logos/{stock.Symbol}.png";

            ApplyPrices(stock.StockDetail, row, now);
        }

        private static StockDetailEntity ApplyPrices(StockDetailEntity detail, NseBhavcopyRow row, DateTime now)
        {
            detail.CurrentPrice = row.Close;
            detail.PreviousClose = row.PreviousClose;
            detail.OpenPrice = row.Open;
            detail.HighPrice = row.High;
            detail.LowPrice = row.Low;
            detail.Volume = row.Volume;
            detail.AverageVolume = RollingAverage(detail.AverageVolume, row.Volume);
            detail.PriceChange = row.Close - row.PreviousClose;
            detail.LastUpdated = now;
            detail.IsActive = true;

            if (row.High > 0)
                detail.Week52High = detail.Week52High <= 0 ? row.High : Math.Max(detail.Week52High, row.High);

            if (row.Low > 0)
                detail.Week52Low = detail.Week52Low <= 0 ? row.Low : Math.Min(detail.Week52Low, row.Low);

            return detail;
        }

        private static long RollingAverage(long existing, long today)
        {
            if (today <= 0)
                return existing;
            if (existing <= 0)
                return today;
            return (existing * 19 + today) / 20;
        }
    }
}

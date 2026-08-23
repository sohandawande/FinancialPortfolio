using FinancialPortfolio.Business.Abstractions.IGoogleSheet;
using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Abstractions.ILogo;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Data.Entities.Stock;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Response.GoogleSheet;
using Microsoft.EntityFrameworkCore;

namespace FinancialPortfolio.Business.Services.GoogleSheet
{
    public sealed class GoogleSheetSyncService : IGoogleSheetSyncService
    {
        private readonly ApplicationDbContext _context;
        private readonly IApplicationLoggerService _logger;
        private readonly TimeProvider _timeProvider;
        private readonly ILogoService _logoService;

        public GoogleSheetSyncService(ApplicationDbContext context, IApplicationLoggerService logger, TimeProvider timeProvider, ILogoService logoService)
        {
            _context = Guard.AgainstNull(context, nameof(context));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _timeProvider = Guard.AgainstNull(timeProvider, nameof(timeProvider));
            _logoService = Guard.AgainstNull(logoService, nameof(logoService));
        }

        public async Task<GoogleSheetSyncResponse> SyncAsync(List<GoogleSheetResponse> response, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(response, nameof(response));
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var existingStocks = await _context.Stocks.Include(x => x.StockDetail).ToDictionaryAsync(x => x.Symbol, cancellationToken);

                int inserted = 0;
                int updated = 0;

                foreach (var item in response)
                {
                    if (existingStocks.TryGetValue(item.Symbol, out var stock))
                    {
                        if (string.IsNullOrWhiteSpace(stock.StockDetail.LogoUrl))
                        {
                            item.LogoUrl = await _logoService.EnsureLogoAsync(stock.Symbol, cancellationToken);
                        }
                        UpdateStock(stock, item);
                        updated++;
                    }
                    else
                    {
                        item.LogoUrl = await _logoService.EnsureLogoAsync(item.Symbol, cancellationToken);
                        CreateStock(item);
                        inserted++;
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Google Sheet Sync completed. Total Records: {response.Count}, Inserted: {inserted}, Updated: {updated}", cancellationToken);

                return new GoogleSheetSyncResponse
                {
                    TotalRecords = response.Count,
                    InsertedRecords = inserted,
                    UpdatedRecords = updated,
                    SkippedRecords = 0          // will be overwritten by caller
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        private void CreateStock(GoogleSheetResponse response)
        {
            var stock = new StockEntity
            {
                Symbol = response.Symbol,
                CompanyName = response.CompanyName,
                Industry = response.Industry,
                ISINCode = response.ISINCode,
                Series = response.Series
            };

            stock.StockDetail = new StockDetailEntity
            {
                Category = response.Category,
                CurrentPrice = response.CurrentPrice,
                PreviousClose = response.PreviousClose,
                OpenPrice = response.OpenPrice,
                HighPrice = response.HighPrice,
                LowPrice = response.LowPrice,
                Volume = response.Volume,
                AverageVolume = response.AverageVolume,
                Week52High = response.Week52High,
                Week52Low = response.Week52Low,
                PE = response.PE,
                EPS = response.EPS,
                MarketCap = response.MarketCap,
                PriceChange = response.PriceChange,
                IsActive = response.IsActive,
                LogoUrl = response.LogoUrl,
                LastUpdated = response.LastUpdated
            };

            _context.Stocks.Add(stock);
        }

        private void UpdateStock(StockEntity stock, GoogleSheetResponse response)
        {
            stock.CompanyName = response.CompanyName;
            stock.Industry = response.Industry;
            stock.ISINCode = response.ISINCode;
            stock.Series = response.Series;

            if (stock.StockDetail is null)
            {
                stock.StockDetail = new StockDetailEntity
                {
                    Category = response.Category,
                    CurrentPrice = response.CurrentPrice,
                    PreviousClose = response.PreviousClose,
                    OpenPrice = response.OpenPrice,
                    HighPrice = response.HighPrice,
                    LowPrice = response.LowPrice,
                    Volume = response.Volume,
                    AverageVolume = response.AverageVolume,
                    Week52High = response.Week52High,
                    Week52Low = response.Week52Low,
                    PE = response.PE,
                    EPS = response.EPS,
                    MarketCap = response.MarketCap,
                    PriceChange = response.PriceChange,
                    IsActive = response.IsActive,
                    LastUpdated = response.LastUpdated
                };
                return;
            }

            stock.StockDetail.Category = response.Category;
            stock.StockDetail.CurrentPrice = response.CurrentPrice;
            stock.StockDetail.PreviousClose = response.PreviousClose;
            stock.StockDetail.OpenPrice = response.OpenPrice;
            stock.StockDetail.HighPrice = response.HighPrice;
            stock.StockDetail.LowPrice = response.LowPrice;
            stock.StockDetail.Volume = response.Volume;
            stock.StockDetail.AverageVolume = response.AverageVolume;
            stock.StockDetail.Week52High = response.Week52High;
            stock.StockDetail.Week52Low = response.Week52Low;
            stock.StockDetail.PE = response.PE;
            stock.StockDetail.EPS = response.EPS;
            stock.StockDetail.MarketCap = response.MarketCap;
            stock.StockDetail.PriceChange = response.PriceChange;
            stock.StockDetail.IsActive = response.IsActive;
            stock.StockDetail.LogoUrl = response.LogoUrl;
            stock.StockDetail.LastUpdated = response.LastUpdated;
        }
    }
}
using AutoMapper;
using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Abstractions.IPortfolio;
using FinancialPortfolio.Business.Abstractions.IValidation;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Data.Entities.Portfolio;
using FinancialPortfolio.Models.Abstractions.ICurrentUser;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Exceptions;
using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Request.Portfolio;
using FinancialPortfolio.Models.Model.Response.Portfolio;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FinancialPortfolio.Business.Services.Portfolio
{
    public sealed class PortfolioService : IPortfolioService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IApplicationLoggerService _logger;
        private readonly IValidationService _validation;
        private readonly ICurrentUserService _currentUser;
        private readonly IHttpContextAccessor _http;

        public PortfolioService(
            ApplicationDbContext context,
            IMapper mapper,
            IApplicationLoggerService logger,
            IValidationService validation,
            ICurrentUserService currentUser,
            IHttpContextAccessor http)
        {
            _context = Guard.AgainstNull(context, nameof(context));
            _mapper = Guard.AgainstNull(mapper, nameof(mapper));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _validation = Guard.AgainstNull(validation, nameof(validation));
            _currentUser = Guard.AgainstNull(currentUser, nameof(currentUser));
            _http = Guard.AgainstNull(http, nameof(http));
        }

        private string? ToPublicLogo(string? logoUrl)
            => LogoUrlHelper.ToPublicUrl(logoUrl, _http.HttpContext?.Request);

        public async Task<ApiResponse<PortfolioResponse?>> GetAsync(CancellationToken cancellationToken)
        {
            try
            {
                var portfolio = await GetPortfolioOrNullAsync(cancellationToken);

                if (portfolio is null)
                    return ResponseFactory.Success<PortfolioResponse?>(null, "No portfolio created yet.");

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    "Portfolio fetched successfully.", cancellationToken);

                return ResponseFactory.Success<PortfolioResponse?>(MapToPortfolioResponse(portfolio), "Portfolio fetched successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<PortfolioResponse>> CreateAsync(CreatePortfolioRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            try
            {
                var existing = await GetPortfolioOrNullAsync(cancellationToken);
                if (existing is not null)
                    throw new ValidationException("A portfolio already exists. Update it instead.");

                var userId = await ResolveUserIdAsync(cancellationToken);
                var name = request.Name.Trim();

                var portfolio = new PortfolioEntity
                {
                    UserId = userId,
                    Name = string.IsNullOrWhiteSpace(name) ? "My Portfolio" : name,
                    Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                    IsActive = true
                };

                await _context.Portfolios.AddAsync(portfolio, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    $"Portfolio '{portfolio.Name}' created.", cancellationToken);

                return ResponseFactory.Success(MapToPortfolioResponse(portfolio), "Portfolio created successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<PortfolioResponse>> UpdateAsync(UpdatePortfolioRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            try
            {
                var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
                if (portfolio is null)
                    throw new NotFoundException("Portfolio not found. Create one first.");

                var name = request.Name.Trim();
                portfolio.Name = string.IsNullOrWhiteSpace(name) ? portfolio.Name : name;
                portfolio.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
                portfolio.IsActive = request.IsActive;

                await _context.SaveChangesAsync(cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    $"Portfolio '{portfolio.Name}' updated.", cancellationToken);

                return ResponseFactory.Success(MapToPortfolioResponse(portfolio), "Portfolio updated successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<PortfolioSummaryResponse>> GetSummaryAsync(CancellationToken cancellationToken)
        {
            try
            {
                var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
                if (portfolio is null)
                    return ResponseFactory.Success(new PortfolioSummaryResponse(), "No portfolio created yet.");

                var holds = await _context.PortfolioStockHolds
                    .AsNoTracking()
                    .Include(h => h.Stock)
                        .ThenInclude(s => s.StockDetail)
                    .Where(h => h.PortfolioId == portfolio.Id)
                    .ToListAsync(cancellationToken);

                var openHolds = holds.Where(h => h.RemainingQuantity > 0).ToList();
                var soldHolds = holds.Where(h => h.IsSold || h.RemainingQuantity == 0).ToList();

                decimal totalInvestment = 0;
                decimal totalCurrentValue = 0;

                foreach (var h in openHolds)
                {
                    var currentPrice = h.Stock?.StockDetail?.CurrentPrice ?? 0;
                    totalInvestment += h.RemainingQuantity * h.PurchasePrice;
                    totalCurrentValue += h.RemainingQuantity * currentPrice;
                }

                var realizedProfit = await _context.PortfolioStockSolds
                    .AsNoTracking()
                    .Where(s => s.PortfolioStockHold.PortfolioId == portfolio.Id)
                    .Join(_context.PortfolioStockHolds,
                        s => s.PortfolioStockHoldId,
                        h => h.Id,
                        (s, h) => (s.SellQuantity * s.SellPrice) - (s.SellQuantity * h.PurchasePrice))
                    .SumAsync(cancellationToken);

                var dividendRows = await _context.PortfolioStockDividends
                    .AsNoTracking()
                    .Where(d => d.PortfolioId == portfolio.Id)
                    .Select(d => new { d.DividendDate, d.Amount })
                    .ToListAsync(cancellationToken);

                var summary = new PortfolioSummaryResponse
                {
                    PortfolioId = portfolio.Id,
                    Name = portfolio.Name,
                    TotalInvestment = Math.Round(totalInvestment, 2),
                    TotalCurrentValue = Math.Round(totalCurrentValue, 2),
                    UnrealizedGainLoss = Math.Round(totalCurrentValue - totalInvestment, 2),
                    UnrealizedGainLossPercent = totalInvestment == 0
                        ? 0
                        : Math.Round(((totalCurrentValue - totalInvestment) / totalInvestment) * 100, 2),
                    RealizedProfitBooked = Math.Round(realizedProfit, 2),
                    TotalDividendsReceived = Math.Round(dividendRows.Sum(d => d.Amount), 2),
                    DividendsByYear = dividendRows
                        .GroupBy(d => d.DividendDate.Year)
                        .OrderByDescending(g => g.Key)
                        .Select(g => new PortfolioDividendYearTotalResponse
                        {
                            Year = g.Key,
                            Amount = Math.Round(g.Sum(x => x.Amount), 2),
                            Count = g.Count()
                        })
                        .ToList(),
                    TotalHoldLots = openHolds.Count,
                    TotalSoldLots = soldHolds.Count,
                    TotalStocksHold = openHolds.Select(h => h.StockId).Distinct().Count(),
                    TotalStocksSold = soldHolds.Select(h => h.StockId).Distinct().Count(),
                    TotalStocksHoldSell = holds.Select(h => h.StockId).Distinct().Count(),
                    LastUpdated = DateTime.UtcNow
                };

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    "Portfolio summary fetched successfully.", cancellationToken);

                return ResponseFactory.Success(summary, "Portfolio summary fetched successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<List<PortfolioHoldingResponse>>> GetHoldingsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
                if (portfolio is null)
                    return ResponseFactory.Success(new List<PortfolioHoldingResponse>(), "No portfolio created yet.");

                var holds = await _context.PortfolioStockHolds
                    .AsNoTracking()
                    .Include(h => h.Stock)
                        .ThenInclude(s => s.StockDetail)
                    .Where(h => h.PortfolioId == portfolio.Id && h.RemainingQuantity > 0)
                    .OrderByDescending(h => h.PurchaseDate)
                    .ToListAsync(cancellationToken);

                var result = holds.Select(MapToHoldingResponse).ToList();

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    "Portfolio holdings fetched successfully.", cancellationToken);

                return ResponseFactory.Success(result, "Holdings fetched successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<List<PortfolioSoldResponse>>> GetSoldHistoryAsync(CancellationToken cancellationToken)
        {
            try
            {
                var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
                if (portfolio is null)
                    return ResponseFactory.Success(new List<PortfolioSoldResponse>(), "No portfolio created yet.");

                var solds = await _context.PortfolioStockSolds
                    .AsNoTracking()
                    .Include(s => s.PortfolioStockHold)
                        .ThenInclude(h => h.Stock)
                            .ThenInclude(st => st.StockDetail)
                    .Where(s => s.PortfolioStockHold.PortfolioId == portfolio.Id)
                    .OrderByDescending(s => s.SoldDate)
                    .ToListAsync(cancellationToken);

                var result = solds.Select(MapToSoldResponse).ToList();

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    "Portfolio sold history fetched successfully.", cancellationToken);

                return ResponseFactory.Success(result, "Sold history fetched successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<PortfolioHoldingResponse>> BuyAsync(BuyStockRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var portfolio = await RequirePortfolioAsync(cancellationToken);

                var stock = await _context.Stocks
                    .Include(s => s.StockDetail)
                    .FirstOrDefaultAsync(s => s.Id == request.StockId, cancellationToken);

                if (stock is null)
                    throw new NotFoundException("Stock not found.");

                var hold = new PortfolioStockHoldEntity
                {
                    PortfolioId = portfolio.Id,
                    StockId = request.StockId,
                    Exchange = request.Exchange,
                    Quantity = request.Quantity,
                    RemainingQuantity = request.Quantity,
                    PurchasePrice = request.PurchasePrice,
                    InvestmentAmount = request.Quantity * request.PurchasePrice,
                    HoldDays = null,
                    LotStatus = LotStatus.Open,
                    IsSold = false,
                    PurchaseDate = request.PurchaseDate.Date,
                    HoldNotes = request.Notes
                };

                await _context.PortfolioStockHolds.AddAsync(hold, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                hold = await _context.PortfolioStockHolds
                    .AsNoTracking()
                    .Include(h => h.Stock)
                        .ThenInclude(s => s.StockDetail)
                    .FirstAsync(h => h.Id == hold.Id, cancellationToken);

                var response = MapToHoldingResponse(hold);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    $"Bought {request.Quantity} of {stock.Symbol}", cancellationToken);

                return ResponseFactory.Success(response, "Stock bought successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<PortfolioHoldingResponse>> UpdateHoldAsync(
            long holdId,
            UpdateHoldRequest request,
            CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var portfolio = await RequirePortfolioAsync(cancellationToken);

                var hold = await _context.PortfolioStockHolds
                    .Include(h => h.PortfolioStockSolds)
                    .Include(h => h.Stock)
                        .ThenInclude(s => s.StockDetail)
                    .FirstOrDefaultAsync(h => h.Id == holdId && h.PortfolioId == portfolio.Id, cancellationToken);

                if (hold is null)
                    throw new NotFoundException("Buy lot not found.");

                var soldQty = hold.PortfolioStockSolds.Sum(s => s.SellQuantity);
                if (request.Quantity < soldQty)
                {
                    throw new ValidationException(
                        $"Cannot set quantity to {request.Quantity}. This lot already has {soldQty} share(s) sold. Edit or delete those sells first.");
                }

                var purchaseDate = request.PurchaseDate.Date;
                if (hold.PortfolioStockSolds.Any(s => s.SoldDate.Date < purchaseDate))
                {
                    throw new ValidationException(
                        "Purchase date cannot be after an existing sell date on this lot.");
                }

                hold.Quantity = request.Quantity;
                hold.PurchasePrice = request.PurchasePrice;
                hold.PurchaseDate = purchaseDate;
                hold.Exchange = request.Exchange;
                hold.HoldNotes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
                hold.InvestmentAmount = request.Quantity * request.PurchasePrice;

                RecalcHoldFromSells(hold);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                hold = await _context.PortfolioStockHolds
                    .AsNoTracking()
                    .Include(h => h.Stock)
                        .ThenInclude(s => s.StockDetail)
                    .FirstAsync(h => h.Id == hold.Id, cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    $"Updated buy lot {holdId} for {hold.Stock.Symbol}.", cancellationToken);

                return ResponseFactory.Success(MapToHoldingResponse(hold), "Buy lot updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<bool>> DeleteHoldAsync(long holdId, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var portfolio = await RequirePortfolioAsync(cancellationToken);

                var hold = await _context.PortfolioStockHolds
                    .Include(h => h.PortfolioStockSolds)
                    .Include(h => h.Stock)
                    .FirstOrDefaultAsync(h => h.Id == holdId && h.PortfolioId == portfolio.Id, cancellationToken);

                if (hold is null)
                    throw new NotFoundException("Buy lot not found.");

                var symbol = hold.Stock.Symbol;
                var sellCount = hold.PortfolioStockSolds.Count;

                if (sellCount > 0)
                    _context.PortfolioStockSolds.RemoveRange(hold.PortfolioStockSolds);

                _context.PortfolioStockHolds.Remove(hold);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    $"Deleted buy lot {holdId} of {symbol} and {sellCount} related sell(s).", cancellationToken);

                return ResponseFactory.Success(true, sellCount > 0
                    ? "Buy lot and related sells deleted."
                    : "Buy lot deleted.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<FifoSellResponse>> SellAsync(SellStockRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var portfolio = await RequirePortfolioAsync(cancellationToken);

                var lots = await _context.PortfolioStockHolds
                    .Include(h => h.Stock)
                        .ThenInclude(s => s.StockDetail)
                    .Include(h => h.Portfolio)
                    .Where(h =>
                        h.PortfolioId == portfolio.Id
                        && h.StockId == request.StockId
                        && h.RemainingQuantity > 0
                        && h.PurchaseDate.Date <= request.SoldDate.Date)
                    .OrderBy(h => h.PurchaseDate)
                    .ThenBy(h => h.CreatedDate)
                    .ThenBy(h => h.Id)
                    .ToListAsync(cancellationToken);

                if (lots.Count == 0)
                    throw new NotFoundException("No open lots found for this stock that can be sold on the selected date.");

                if (lots[0].Portfolio.UserId != portfolio.UserId)
                    throw new ForbiddenException("You do not own this holding.");

                var availableQty = lots.Sum(h => h.RemainingQuantity);
                if (availableQty < request.SellQuantity)
                {
                    throw new ValidationException(new[]
                    {
                        $"Cannot sell {request.SellQuantity}. Only {availableQty} available across {lots.Count} lot(s)."
                    });
                }

                var remainingToSell = request.SellQuantity;
                var soldEntities = new List<PortfolioStockSoldEntity>();
                var symbol = lots[0].Stock.Symbol;
                var companyName = lots[0].Stock.CompanyName;

                foreach (var hold in lots)
                {
                    if (remainingToSell <= 0)
                        break;

                    var sellQty = Math.Min(hold.RemainingQuantity, remainingToSell);
                    var holdDays = (request.SoldDate.Date - hold.PurchaseDate.Date).Days;
                    if (holdDays < 0) holdDays = 0;

                    var sold = new PortfolioStockSoldEntity
                    {
                        PortfolioStockHoldId = hold.Id,
                        SellQuantity = sellQty,
                        SellPrice = request.SellPrice,
                        HoldDays = holdDays,
                        SoldDate = request.SoldDate.Date,
                        SoldNotes = request.Notes
                    };

                    hold.RemainingQuantity -= sellQty;
                    hold.HoldDays = holdDays;

                    if (hold.RemainingQuantity == 0)
                    {
                        hold.IsSold = true;
                        hold.LotStatus = LotStatus.FullySold;
                        hold.ExitDate = request.SoldDate.Date;
                        sold.LotStatus = LotStatus.FullySold;
                    }
                    else
                    {
                        hold.IsSold = false;
                        hold.LotStatus = LotStatus.PartiallySold;
                        sold.LotStatus = LotStatus.PartiallySold;
                    }

                    soldEntities.Add(sold);
                    remainingToSell -= sellQty;
                }

                await _context.PortfolioStockSolds.AddRangeAsync(soldEntities, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var soldIds = soldEntities.Select(s => s.Id).ToList();
                var persisted = await _context.PortfolioStockSolds
                    .AsNoTracking()
                    .Include(s => s.PortfolioStockHold)
                        .ThenInclude(h => h.Stock)
                            .ThenInclude(st => st.StockDetail)
                    .Where(s => soldIds.Contains(s.Id))
                    .OrderBy(s => s.PortfolioStockHold.PurchaseDate)
                    .ThenBy(s => s.Id)
                    .ToListAsync(cancellationToken);

                var allocations = persisted.Select(MapToSoldResponse).ToList();
                var totalSellAmount = allocations.Sum(a => a.SellAmount);
                var totalCostAmount = allocations.Sum(a => a.CostAmount);
                var realized = totalSellAmount - totalCostAmount;

                var response = new FifoSellResponse
                {
                    StockId = request.StockId,
                    Symbol = symbol,
                    CompanyName = companyName,
                    TotalSellQuantity = request.SellQuantity,
                    SellPrice = request.SellPrice,
                    TotalSellAmount = Math.Round(totalSellAmount, 2),
                    TotalCostAmount = Math.Round(totalCostAmount, 2),
                    TotalRealizedGainLoss = Math.Round(realized, 2),
                    TotalRealizedGainLossPercent = totalCostAmount == 0
                        ? 0
                        : Math.Round((realized / totalCostAmount) * 100, 2),
                    LotsConsumed = allocations.Count,
                    Allocations = allocations
                };

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    $"Sold {request.SellQuantity} of {symbol} FIFO across {allocations.Count} lot(s).", cancellationToken);

                return ResponseFactory.Success(response, "Stock sold successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<PortfolioSoldResponse>> UpdateSoldAsync(
            long soldId,
            UpdateSoldRequest request,
            CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var portfolio = await RequirePortfolioAsync(cancellationToken);

                var sold = await _context.PortfolioStockSolds
                    .Include(s => s.PortfolioStockHold)
                        .ThenInclude(h => h.PortfolioStockSolds)
                    .Include(s => s.PortfolioStockHold)
                        .ThenInclude(h => h.Stock)
                            .ThenInclude(st => st.StockDetail)
                    .FirstOrDefaultAsync(
                        s => s.Id == soldId && s.PortfolioStockHold.PortfolioId == portfolio.Id,
                        cancellationToken);

                if (sold is null)
                    throw new NotFoundException("Sell record not found.");

                var hold = sold.PortfolioStockHold;
                var otherSoldQty = hold.PortfolioStockSolds
                    .Where(s => s.Id != sold.Id)
                    .Sum(s => s.SellQuantity);

                if (otherSoldQty + request.SellQuantity > hold.Quantity)
                {
                    var max = hold.Quantity - otherSoldQty;
                    throw new ValidationException(
                        $"Cannot set sell quantity to {request.SellQuantity}. This lot only has {max} share(s) available for this sell.");
                }

                var soldDate = request.SoldDate.Date;
                if (soldDate < hold.PurchaseDate.Date)
                    throw new ValidationException("Sold date cannot be before the lot purchase date.");

                sold.SellQuantity = request.SellQuantity;
                sold.SellPrice = request.SellPrice;
                sold.SoldDate = soldDate;
                sold.SoldNotes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

                RecalcHoldFromSells(hold);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                sold = await _context.PortfolioStockSolds
                    .AsNoTracking()
                    .Include(s => s.PortfolioStockHold)
                        .ThenInclude(h => h.Stock)
                            .ThenInclude(st => st.StockDetail)
                    .FirstAsync(s => s.Id == sold.Id, cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    $"Updated sell {soldId} of {sold.PortfolioStockHold.Stock.Symbol}.", cancellationToken);

                return ResponseFactory.Success(MapToSoldResponse(sold), "Sell record updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<bool>> DeleteSoldAsync(long soldId, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var portfolio = await RequirePortfolioAsync(cancellationToken);

                var sold = await _context.PortfolioStockSolds
                    .Include(s => s.PortfolioStockHold)
                        .ThenInclude(h => h.PortfolioStockSolds)
                    .Include(s => s.PortfolioStockHold)
                        .ThenInclude(h => h.Stock)
                    .FirstOrDefaultAsync(
                        s => s.Id == soldId && s.PortfolioStockHold.PortfolioId == portfolio.Id,
                        cancellationToken);

                if (sold is null)
                    throw new NotFoundException("Sell record not found.");

                var hold = sold.PortfolioStockHold;
                var symbol = hold.Stock.Symbol;

                _context.PortfolioStockSolds.Remove(sold);
                hold.PortfolioStockSolds.Remove(sold);
                RecalcHoldFromSells(hold);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    $"Deleted sell {soldId} of {symbol} and restored {sold.SellQuantity} to the lot.", cancellationToken);

                return ResponseFactory.Success(true, "Sell record deleted. Quantity restored to the buy lot.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<List<PortfolioLedgerItemResponse>>> GetLedgerAsync(string? type, CancellationToken cancellationToken)
        {
            try
            {
                var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
                if (portfolio is null)
                    return ResponseFactory.Success(new List<PortfolioLedgerItemResponse>(), "No portfolio created yet.");

                var today = DateTime.UtcNow.Date;
                var filter = (type ?? "lifetime").Trim().ToLowerInvariant();

                var holds = await _context.PortfolioStockHolds
                    .AsNoTracking()
                    .Include(h => h.Stock)
                        .ThenInclude(s => s.StockDetail)
                    .Where(h => h.PortfolioId == portfolio.Id)
                    .ToListAsync(cancellationToken);

                var solds = await _context.PortfolioStockSolds
                    .AsNoTracking()
                    .Include(s => s.PortfolioStockHold)
                        .ThenInclude(h => h.Stock)
                            .ThenInclude(st => st.StockDetail)
                    .Where(s => s.PortfolioStockHold.PortfolioId == portfolio.Id)
                    .ToListAsync(cancellationToken);

                var rows = new List<PortfolioLedgerItemResponse>();

                foreach (var hold in holds.Where(h => h.RemainingQuantity > 0))
                {
                    var currentPrice = hold.Stock?.StockDetail?.CurrentPrice ?? 0m;
                    var investment = hold.RemainingQuantity * hold.PurchasePrice;
                    var currentValue = hold.RemainingQuantity * currentPrice;
                    var pnl = currentValue - investment;
                    var days = (today - hold.PurchaseDate.Date).Days;
                    if (days < 0) days = 0;

                    rows.Add(new PortfolioLedgerItemResponse
                    {
                        HoldId = hold.Id,
                        Id = hold.Id,
                        StockId = hold.StockId,
                        CompanyName = hold.Stock?.CompanyName ?? string.Empty,
                        Symbol = hold.Stock?.Symbol ?? string.Empty,
                        StockCode = $"{hold.Exchange}:{hold.Stock?.Symbol}",
                        LogoUrl = ToPublicLogo(hold.Stock?.StockDetail?.LogoUrl),
                        Exchange = hold.Exchange,
                        NetQuantity = hold.RemainingQuantity,
                        PurchasePrice = hold.PurchasePrice,
                        MarketPrice = currentPrice,
                        TotalInvestment = Math.Round(investment, 2),
                        TotalCurrentValue = Math.Round(currentValue, 2),
                        TotalGainLoss = Math.Round(pnl, 2),
                        GainLossPercent = investment == 0 ? 0 : Math.Round((pnl / investment) * 100, 2),
                        HoldDays = days,
                        CurrentType = InvestmentAction.Hold,
                        AsOfDate = today,
                        PurchaseDate = hold.PurchaseDate,
                        ExitDate = null,
                        SellPrice = null,
                        TotalOnSell = null,
                        ProfitLoss = pnl > 0 ? ProfitLoss.Profit : pnl < 0 ? ProfitLoss.Loss : ProfitLoss.Equal
                    });
                }

                foreach (var sold in solds)
                {
                    var hold = sold.PortfolioStockHold;
                    var cost = sold.SellQuantity * hold.PurchasePrice;
                    var sellAmount = sold.SellQuantity * sold.SellPrice;
                    var pnl = sellAmount - cost;
                    var days = sold.HoldDays ?? (sold.SoldDate.Date - hold.PurchaseDate.Date).Days;
                    if (days < 0) days = 0;

                    rows.Add(new PortfolioLedgerItemResponse
                    {
                        SoldId = sold.Id,
                        HoldId = hold.Id,
                        Id = sold.Id,
                        StockId = hold.StockId,
                        CompanyName = hold.Stock?.CompanyName ?? string.Empty,
                        Symbol = hold.Stock?.Symbol ?? string.Empty,
                        StockCode = $"{hold.Exchange}:{hold.Stock?.Symbol}",
                        LogoUrl = ToPublicLogo(hold.Stock?.StockDetail?.LogoUrl),
                        Exchange = hold.Exchange,
                        NetQuantity = sold.SellQuantity,
                        PurchasePrice = hold.PurchasePrice,
                        MarketPrice = hold.Stock?.StockDetail?.CurrentPrice ?? sold.SellPrice,
                        TotalInvestment = Math.Round(cost, 2),
                        TotalCurrentValue = Math.Round(sellAmount, 2),
                        TotalGainLoss = Math.Round(pnl, 2),
                        GainLossPercent = cost == 0 ? 0 : Math.Round((pnl / cost) * 100, 2),
                        HoldDays = days,
                        CurrentType = InvestmentAction.Sell,
                        AsOfDate = today,
                        PurchaseDate = hold.PurchaseDate,
                        ExitDate = sold.SoldDate,
                        SellPrice = sold.SellPrice,
                        TotalOnSell = Math.Round(sellAmount, 2),
                        ProfitLoss = pnl > 0 ? ProfitLoss.Profit : pnl < 0 ? ProfitLoss.Loss : ProfitLoss.Equal
                    });
                }

                var ordered = rows
                    .OrderBy(r => r.PurchaseDate)
                    .ThenBy(r => r.CurrentType)
                    .ThenBy(r => r.Id)
                    .ToList();

                for (var i = 0; i < ordered.Count; i++)
                    ordered[i].SerialNo = i + 1;

                var filtered = filter switch
                {
                    "hold" => ordered.Where(r => r.CurrentType == InvestmentAction.Hold).ToList(),
                    "sell" => ordered.Where(r => r.CurrentType == InvestmentAction.Sell).ToList(),
                    _ => ordered
                };

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    $"Portfolio ledger fetched. Filter={filter}, Rows={filtered.Count}.", cancellationToken);

                return ResponseFactory.Success(filtered, "Ledger fetched successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<List<PortfolioPositionResponse>>> GetPositionsAsync(string? status, CancellationToken cancellationToken)
        {
            try
            {
                var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
                if (portfolio is null)
                    return ResponseFactory.Success(new List<PortfolioPositionResponse>(), "No portfolio created yet.");

                var today = DateTime.UtcNow.Date;
                var filter = (status ?? "all").Trim().ToLowerInvariant();

                var holds = await _context.PortfolioStockHolds
                    .AsNoTracking()
                    .Include(h => h.Stock)
                        .ThenInclude(s => s.StockDetail)
                    .Where(h => h.PortfolioId == portfolio.Id)
                    .ToListAsync(cancellationToken);

                var solds = await _context.PortfolioStockSolds
                    .AsNoTracking()
                    .Include(s => s.PortfolioStockHold)
                    .Where(s => s.PortfolioStockHold.PortfolioId == portfolio.Id)
                    .ToListAsync(cancellationToken);

                var dividends = await _context.PortfolioStockDividends
                    .AsNoTracking()
                    .Where(d => d.PortfolioId == portfolio.Id)
                    .ToListAsync(cancellationToken);

                var positions = holds
                    .GroupBy(h => h.StockId)
                    .Select(g => MapToPositionResponse(
                        g.ToList(),
                        solds.Where(s => s.PortfolioStockHold.StockId == g.Key).ToList(),
                        dividends.Where(d => d.StockId == g.Key).ToList(),
                        today))
                    .OrderBy(p => p.CompanyName)
                    .ThenBy(p => p.Symbol)
                    .ToList();

                var filtered = filter switch
                {
                    "holding" or "hold" => positions.Where(p => p.Status == PositionStatus.Holding).ToList(),
                    "sold" => positions.Where(p => p.Status == PositionStatus.FullySold).ToList(),
                    _ => positions
                };

                for (var i = 0; i < filtered.Count; i++)
                    filtered[i].SerialNo = i + 1;

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    $"Portfolio positions fetched. Filter={filter}, Rows={filtered.Count}.", cancellationToken);

                return ResponseFactory.Success(filtered, "Positions fetched successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<PortfolioPositionDetailResponse>> GetPositionDetailAsync(long stockId, CancellationToken cancellationToken)
        {
            try
            {
                var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
                if (portfolio is null)
                    throw new NotFoundException("Portfolio not found. Create one first.");

                var holds = await _context.PortfolioStockHolds
                    .AsNoTracking()
                    .Include(h => h.Stock)
                        .ThenInclude(s => s.StockDetail)
                    .Where(h => h.PortfolioId == portfolio.Id && h.StockId == stockId)
                    .OrderBy(h => h.PurchaseDate)
                    .ThenBy(h => h.Id)
                    .ToListAsync(cancellationToken);

                if (holds.Count == 0)
                    throw new NotFoundException("No buy/sell history found for this stock in your portfolio.");

                var solds = await _context.PortfolioStockSolds
                    .AsNoTracking()
                    .Include(s => s.PortfolioStockHold)
                        .ThenInclude(h => h.Stock)
                            .ThenInclude(st => st.StockDetail)
                    .Where(s => s.PortfolioStockHold.PortfolioId == portfolio.Id && s.PortfolioStockHold.StockId == stockId)
                    .OrderBy(s => s.SoldDate)
                    .ThenBy(s => s.Id)
                    .ToListAsync(cancellationToken);

                var dividends = await _context.PortfolioStockDividends
                    .AsNoTracking()
                    .Include(d => d.Stock)
                        .ThenInclude(s => s.StockDetail)
                    .Where(d => d.PortfolioId == portfolio.Id && d.StockId == stockId)
                    .OrderBy(d => d.DividendDate)
                    .ThenBy(d => d.Id)
                    .ToListAsync(cancellationToken);

                var today = DateTime.UtcNow.Date;
                var position = MapToPositionResponse(holds, solds, dividends, today);
                position.SerialNo = 1;

                var timeline = new List<PortfolioPositionEventResponse>();
                timeline.AddRange(holds.Select(h => new PortfolioPositionEventResponse
                {
                    EventType = "Buy",
                    EventDate = h.PurchaseDate,
                    Quantity = h.Quantity,
                    Price = h.PurchasePrice,
                    Amount = h.InvestmentAmount,
                    Notes = h.HoldNotes,
                    SourceId = h.Id
                }));
                timeline.AddRange(solds.Select(s => new PortfolioPositionEventResponse
                {
                    EventType = "Sell",
                    EventDate = s.SoldDate,
                    Quantity = s.SellQuantity,
                    Price = s.SellPrice,
                    Amount = s.SellQuantity * s.SellPrice,
                    Notes = s.SoldNotes,
                    SourceId = s.Id
                }));
                timeline.AddRange(dividends.Select(d => new PortfolioPositionEventResponse
                {
                    EventType = "Dividend",
                    EventDate = d.DividendDate,
                    Quantity = d.Quantity,
                    Price = d.PerShareAmount,
                    Amount = d.Amount,
                    Notes = d.Notes,
                    SourceId = d.Id
                }));

                var detail = new PortfolioPositionDetailResponse
                {
                    Position = position,
                    Buys = holds.Select(MapToHoldingResponse).ToList(),
                    Sells = solds.Select(MapToSoldResponse).ToList(),
                    Dividends = dividends.Select(d => MapToDividendResponse(d, holds[0].Exchange)).ToList(),
                    DividendsByYear = GroupDividendsByYear(dividends),
                    Timeline = timeline
                        .OrderBy(e => e.EventDate)
                        .ThenBy(e => e.EventType)
                        .ThenBy(e => e.SourceId)
                        .ToList()
                };

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    $"Position detail fetched for stock {stockId}.", cancellationToken);

                return ResponseFactory.Success(detail, "Position detail fetched successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<List<PortfolioDividendResponse>>> GetDividendsAsync(long? stockId, CancellationToken cancellationToken)
        {
            try
            {
                var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
                if (portfolio is null)
                    return ResponseFactory.Success(new List<PortfolioDividendResponse>(), "No portfolio created yet.");

                var query = _context.PortfolioStockDividends
                    .AsNoTracking()
                    .Include(d => d.Stock)
                        .ThenInclude(s => s.StockDetail)
                    .Where(d => d.PortfolioId == portfolio.Id);

                if (stockId.HasValue && stockId.Value > 0)
                    query = query.Where(d => d.StockId == stockId.Value);

                var dividends = await query
                    .OrderByDescending(d => d.DividendDate)
                    .ThenByDescending(d => d.Id)
                    .ToListAsync(cancellationToken);

                var holdExchanges = await _context.PortfolioStockHolds
                    .AsNoTracking()
                    .Where(h => h.PortfolioId == portfolio.Id)
                    .GroupBy(h => h.StockId)
                    .Select(g => new { StockId = g.Key, Exchange = g.Max(x => x.Exchange) })
                    .ToDictionaryAsync(x => x.StockId, x => x.Exchange, cancellationToken);

                var result = dividends.Select(d =>
                {
                    holdExchanges.TryGetValue(d.StockId, out var exchange);
                    return MapToDividendResponse(d, exchange);
                }).ToList();

                return ResponseFactory.Success(result, "Dividends fetched successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<PortfolioDividendResponse>> AddDividendAsync(AddDividendRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            try
            {
                var portfolio = await RequirePortfolioAsync(cancellationToken);

                var hasTraded = await _context.PortfolioStockHolds
                    .AsNoTracking()
                    .AnyAsync(h => h.PortfolioId == portfolio.Id && h.StockId == request.StockId, cancellationToken);

                if (!hasTraded)
                    throw new ValidationException("Record at least one buy of this stock before adding a dividend.");

                var stock = await _context.Stocks
                    .Include(s => s.StockDetail)
                    .FirstOrDefaultAsync(s => s.Id == request.StockId, cancellationToken);

                if (stock is null)
                    throw new NotFoundException("Stock not found.");

                var perShare = request.PerShareAmount;
                var amount = request.Amount ?? 0m;
                if (perShare <= 0 && amount > 0 && request.Quantity > 0)
                    perShare = Math.Round(amount / request.Quantity, 4);
                if (amount <= 0)
                    amount = Math.Round(request.Quantity * perShare, 2);

                var entity = new PortfolioStockDividendEntity
                {
                    PortfolioId = portfolio.Id,
                    StockId = request.StockId,
                    Quantity = request.Quantity,
                    PerShareAmount = perShare,
                    Amount = amount,
                    DividendDate = request.DividendDate.Date,
                    ExDate = request.ExDate?.Date,
                    RecordDate = request.RecordDate?.Date,
                    Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
                };

                await _context.PortfolioStockDividends.AddAsync(entity, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                entity = await _context.PortfolioStockDividends
                    .AsNoTracking()
                    .Include(d => d.Stock)
                        .ThenInclude(s => s.StockDetail)
                    .FirstAsync(d => d.Id == entity.Id, cancellationToken);

                var exchange = await _context.PortfolioStockHolds
                    .AsNoTracking()
                    .Where(h => h.PortfolioId == portfolio.Id && h.StockId == request.StockId)
                    .Select(h => h.Exchange)
                    .FirstOrDefaultAsync(cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    $"Dividend {amount} recorded for {stock.Symbol}.", cancellationToken);

                return ResponseFactory.Success(MapToDividendResponse(entity, exchange), "Dividend recorded successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<bool>> DeleteDividendAsync(long dividendId, CancellationToken cancellationToken)
        {
            try
            {
                var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
                if (portfolio is null)
                    throw new NotFoundException("Portfolio not found.");

                var entity = await _context.PortfolioStockDividends
                    .FirstOrDefaultAsync(d => d.Id == dividendId && d.PortfolioId == portfolio.Id, cancellationToken);

                if (entity is null)
                    throw new NotFoundException("Dividend not found.");

                _context.PortfolioStockDividends.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    $"Dividend {dividendId} deleted.", cancellationToken);

                return ResponseFactory.Success(true, "Dividend deleted successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<PortfolioDividendOverviewResponse>> GetDividendOverviewAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
                if (portfolio is null)
                {
                    return ResponseFactory.Success(
                        new PortfolioDividendOverviewResponse(),
                        "No portfolio created yet.");
                }

                var dividends = await _context.PortfolioStockDividends
                    .AsNoTracking()
                    .Include(d => d.Stock)
                        .ThenInclude(s => s.StockDetail)
                    .Where(d => d.PortfolioId == portfolio.Id)
                    .OrderByDescending(d => d.DividendDate)
                    .ThenByDescending(d => d.Id)
                    .ToListAsync(cancellationToken);

                var holdExchanges = await _context.PortfolioStockHolds
                    .AsNoTracking()
                    .Where(h => h.PortfolioId == portfolio.Id)
                    .GroupBy(h => h.StockId)
                    .Select(g => new { StockId = g.Key, Exchange = g.Max(x => x.Exchange) })
                    .ToDictionaryAsync(x => x.StockId, x => x.Exchange, cancellationToken);

                var payouts = dividends.Select(d =>
                {
                    holdExchanges.TryGetValue(d.StockId, out var exchange);
                    return MapToDividendResponse(d, exchange);
                }).ToList();

                var stocks = payouts
                    .GroupBy(d => d.StockId)
                    .Select(g =>
                    {
                        var first = g.First();
                        var ordered = g
                            .OrderByDescending(x => x.DividendDate)
                            .ThenByDescending(x => x.Id)
                            .ToList();

                        return new PortfolioDividendStockGroupResponse
                        {
                            StockId = first.StockId,
                            Symbol = first.Symbol,
                            CompanyName = first.CompanyName,
                            LogoUrl = first.LogoUrl,
                            Exchange = first.Exchange,
                            TotalAmount = Math.Round(g.Sum(x => x.Amount), 2),
                            PayoutCount = g.Count(),
                            TotalShares = g.Max(x => x.Quantity),
                            LastDividendDate = ordered[0].DividendDate,
                            Payouts = ordered
                        };
                    })
                    .OrderByDescending(g => g.TotalAmount)
                    .ThenBy(g => g.CompanyName)
                    .ToList();

                var years = payouts
                    .GroupBy(d => d.DividendDate.Year)
                    .Select(g => new PortfolioDividendYearGroupResponse
                    {
                        Year = g.Key,
                        Amount = Math.Round(g.Sum(x => x.Amount), 2),
                        PayoutCount = g.Count(),
                        CompanyCount = g.Select(x => x.StockId).Distinct().Count(),
                        Payouts = g
                            .OrderByDescending(x => x.DividendDate)
                            .ThenByDescending(x => x.Id)
                            .ToList()
                    })
                    .OrderByDescending(y => y.Year)
                    .ToList();

                var overview = new PortfolioDividendOverviewResponse
                {
                    TotalAmount = Math.Round(payouts.Sum(x => x.Amount), 2),
                    CompanyCount = stocks.Count,
                    PayoutCount = payouts.Count,
                    Stocks = stocks,
                    Years = years,
                    Payouts = payouts
                };

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    "Dividend overview fetched successfully.", cancellationToken);

                return ResponseFactory.Success(overview, "Dividend overview fetched successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<PortfolioDividendResponse>> UpdateDividendAsync(
            long dividendId,
            UpdateDividendRequest request,
            CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            try
            {
                var portfolio = await RequirePortfolioAsync(cancellationToken);

                var entity = await _context.PortfolioStockDividends
                    .FirstOrDefaultAsync(d => d.Id == dividendId && d.PortfolioId == portfolio.Id, cancellationToken);

                if (entity is null)
                    throw new NotFoundException("Dividend not found.");

                var hasTraded = await _context.PortfolioStockHolds
                    .AsNoTracking()
                    .AnyAsync(h => h.PortfolioId == portfolio.Id && h.StockId == request.StockId, cancellationToken);

                if (!hasTraded)
                    throw new ValidationException("Record at least one buy of this stock before updating the dividend.");

                var stock = await _context.Stocks
                    .Include(s => s.StockDetail)
                    .FirstOrDefaultAsync(s => s.Id == request.StockId, cancellationToken);

                if (stock is null)
                    throw new NotFoundException("Stock not found.");

                var perShare = request.PerShareAmount;
                var amount = request.Amount ?? 0m;
                if (perShare <= 0 && amount > 0 && request.Quantity > 0)
                    perShare = Math.Round(amount / request.Quantity, 4);
                if (amount <= 0)
                    amount = Math.Round(request.Quantity * perShare, 2);

                entity.StockId = request.StockId;
                entity.Quantity = request.Quantity;
                entity.PerShareAmount = perShare;
                entity.Amount = amount;
                entity.DividendDate = request.DividendDate.Date;
                entity.ExDate = request.ExDate?.Date;
                entity.RecordDate = request.RecordDate?.Date;
                entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

                await _context.SaveChangesAsync(cancellationToken);

                entity = await _context.PortfolioStockDividends
                    .AsNoTracking()
                    .Include(d => d.Stock)
                        .ThenInclude(s => s.StockDetail)
                    .FirstAsync(d => d.Id == entity.Id, cancellationToken);

                var exchange = await _context.PortfolioStockHolds
                    .AsNoTracking()
                    .Where(h => h.PortfolioId == portfolio.Id && h.StockId == request.StockId)
                    .Select(h => h.Exchange)
                    .FirstOrDefaultAsync(cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    $"Dividend {dividendId} updated to {amount} for {stock.Symbol}.", cancellationToken);

                return ResponseFactory.Success(MapToDividendResponse(entity, exchange), "Dividend updated successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(),
                    ex.Message, ex, cancellationToken);
                throw;
            }
        }

        private async Task<long> ResolveUserIdAsync(CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            if (userId <= 0)
                throw new UnauthorizedException("Invalid user context. Please login again.");

            var appUserExists = await _context.AppUsers
                .AsNoTracking()
                .AnyAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

            if (appUserExists)
                return userId;

            var appUser = await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdentityUserId == _currentUser.IdentityUserId && !u.IsDeleted, cancellationToken);

            if (appUser is null)
                throw new NotFoundException("User profile not found. Please complete registration.");

            return appUser.Id;
        }

        private async Task<PortfolioEntity?> GetPortfolioOrNullAsync(CancellationToken cancellationToken)
        {
            var userId = await ResolveUserIdAsync(cancellationToken);

            return await _context.Portfolios
                .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive, cancellationToken);
        }

        private async Task<PortfolioEntity> RequirePortfolioAsync(CancellationToken cancellationToken)
        {
            var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
            if (portfolio is not null)
                return portfolio;

            var userId = await ResolveUserIdAsync(cancellationToken);
            portfolio = new PortfolioEntity
            {
                UserId = userId,
                Name = "My Portfolio",
                IsActive = true
            };

            await _context.Portfolios.AddAsync(portfolio, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return portfolio;
        }

        private PortfolioResponse MapToPortfolioResponse(PortfolioEntity p)
        {
            return new PortfolioResponse
            {
                Id = p.Id,
                UserId = p.UserId,
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive,
                CreatedBy = p.CreatedBy,
                ModifiedBy = p.ModifiedBy,
                CreatedDate = p.CreatedDate,
                ModifiedDate = p.ModifiedDate
            };
        }

        private PortfolioHoldingResponse MapToHoldingResponse(PortfolioStockHoldEntity h)
        {
            var currentPrice = h.Stock?.StockDetail?.CurrentPrice ?? 0m;
            var remainingInvestment = h.RemainingQuantity * h.PurchasePrice;
            var currentValue = h.RemainingQuantity * currentPrice;
            var unrealized = currentValue - remainingInvestment;

            return new PortfolioHoldingResponse
            {
                Id = h.Id,
                PortfolioId = h.PortfolioId,
                StockId = h.StockId,
                Symbol = h.Stock?.Symbol ?? string.Empty,
                CompanyName = h.Stock?.CompanyName ?? string.Empty,
                LogoUrl = ToPublicLogo(h.Stock?.StockDetail?.LogoUrl),
                Exchange = h.Exchange,
                Quantity = h.Quantity,
                RemainingQuantity = h.RemainingQuantity,
                PurchasePrice = h.PurchasePrice,
                InvestmentAmount = h.InvestmentAmount,
                RemainingInvestment = Math.Round(remainingInvestment, 2),
                CurrentPrice = currentPrice,
                CurrentValue = Math.Round(currentValue, 2),
                UnrealizedGainLoss = Math.Round(unrealized, 2),
                UnrealizedGainLossPercent = remainingInvestment == 0
                    ? 0
                    : Math.Round((unrealized / remainingInvestment) * 100, 2),
                HoldDays = ComputeHoldDays(h.PurchaseDate),
                LotStatus = h.LotStatus,
                IsSold = h.IsSold,
                PurchaseDate = h.PurchaseDate,
                ExitDate = h.ExitDate,
                HoldNotes = h.HoldNotes,
                CreatedBy = h.CreatedBy,
                ModifiedBy = h.ModifiedBy,
                CreatedDate = h.CreatedDate,
                ModifiedDate = h.ModifiedDate
            };
        }

        private PortfolioSoldResponse MapToSoldResponse(PortfolioStockSoldEntity s)
        {
            var hold = s.PortfolioStockHold;
            var costAmount = s.SellQuantity * hold.PurchasePrice;
            var sellAmount = s.SellQuantity * s.SellPrice;
            var realized = sellAmount - costAmount;

            return new PortfolioSoldResponse
            {
                Id = s.Id,
                HoldId = s.PortfolioStockHoldId,
                StockId = hold.StockId,
                Symbol = hold.Stock?.Symbol ?? string.Empty,
                CompanyName = hold.Stock?.CompanyName ?? string.Empty,
                LogoUrl = ToPublicLogo(hold.Stock?.StockDetail?.LogoUrl),
                Exchange = hold.Exchange,
                SellQuantity = s.SellQuantity,
                SellPrice = s.SellPrice,
                SellAmount = Math.Round(sellAmount, 2),
                PurchasePrice = hold.PurchasePrice,
                CostAmount = Math.Round(costAmount, 2),
                RealizedGainLoss = Math.Round(realized, 2),
                RealizedGainLossPercent = costAmount == 0
                    ? 0
                    : Math.Round((realized / costAmount) * 100, 2),
                HoldDays = s.HoldDays ?? ComputeHoldDays(hold.PurchaseDate, s.SoldDate),
                LotStatus = s.LotStatus,
                PurchaseDate = hold.PurchaseDate,
                SoldDate = s.SoldDate,
                SoldNotes = s.SoldNotes,
                CreatedBy = s.CreatedBy,
                ModifiedBy = s.ModifiedBy,
                CreatedDate = s.CreatedDate,
                ModifiedDate = s.ModifiedDate
            };
        }

        private PortfolioDividendResponse MapToDividendResponse(PortfolioStockDividendEntity d, StockExchange exchange)
        {
            return new PortfolioDividendResponse
            {
                Id = d.Id,
                PortfolioId = d.PortfolioId,
                StockId = d.StockId,
                Symbol = d.Stock?.Symbol ?? string.Empty,
                CompanyName = d.Stock?.CompanyName ?? string.Empty,
                LogoUrl = ToPublicLogo(d.Stock?.StockDetail?.LogoUrl),
                Exchange = exchange,
                Quantity = d.Quantity,
                PerShareAmount = d.PerShareAmount,
                Amount = d.Amount,
                DividendDate = d.DividendDate,
                ExDate = d.ExDate,
                RecordDate = d.RecordDate,
                Notes = d.Notes,
                CreatedBy = d.CreatedBy,
                ModifiedBy = d.ModifiedBy,
                CreatedDate = d.CreatedDate,
                ModifiedDate = d.ModifiedDate
            };
        }

        private PortfolioPositionResponse MapToPositionResponse(
            List<PortfolioStockHoldEntity> holds,
            List<PortfolioStockSoldEntity> solds,
            List<PortfolioStockDividendEntity> dividends,
            DateTime today)
        {
            var first = holds[0];
            var remainingQty = holds.Sum(h => h.RemainingQuantity);
            var remainingInvestment = holds.Sum(h => h.RemainingQuantity * h.PurchasePrice);
            var boughtQty = holds.Sum(h => h.Quantity);
            var lifetimeInvestment = holds.Sum(h => h.InvestmentAmount);
            var soldQty = solds.Sum(s => s.SellQuantity);
            var sellAmount = solds.Sum(s => s.SellQuantity * s.SellPrice);
            var soldCost = solds.Sum(s =>
            {
                var hold = s.PortfolioStockHold ?? holds.First(h => h.Id == s.PortfolioStockHoldId);
                return s.SellQuantity * hold.PurchasePrice;
            });
            var realized = sellAmount - soldCost;
            var dividendTotal = dividends.Sum(d => d.Amount);
            var currentPrice = first.Stock?.StockDetail?.CurrentPrice ?? 0m;
            var currentValue = remainingQty * currentPrice;
            var unrealized = currentValue - remainingInvestment;
            var avgBuy = remainingQty > 0
                ? remainingInvestment / remainingQty
                : boughtQty > 0 ? lifetimeInvestment / boughtQty : 0m;
            var lifetimeAvg = boughtQty > 0 ? lifetimeInvestment / boughtQty : 0m;
            var avgSell = soldQty > 0 ? sellAmount / soldQty : (decimal?)null;
            var firstBuy = holds.Min(h => h.PurchaseDate.Date);
            var lastExit = solds.Count > 0 ? solds.Max(s => s.SoldDate.Date) : (DateTime?)null;
            var lastActivity = firstBuy;
            if (holds.Count > 0) lastActivity = holds.Max(h => h.PurchaseDate.Date);
            if (solds.Count > 0) lastActivity = MaxDate(lastActivity, solds.Max(s => s.SoldDate.Date));
            if (dividends.Count > 0) lastActivity = MaxDate(lastActivity, dividends.Max(d => d.DividendDate.Date));

            var days = remainingQty > 0
                ? (today - firstBuy).Days
                : lastExit.HasValue ? (lastExit.Value - firstBuy).Days : 0;
            if (days < 0) days = 0;

            var status = remainingQty > 0 ? PositionStatus.Holding : PositionStatus.FullySold;
            var totalPnl = realized + unrealized + dividendTotal;
            var costBase = remainingInvestment + soldCost;
            if (costBase == 0) costBase = lifetimeInvestment;

            return new PortfolioPositionResponse
            {
                StockId = first.StockId,
                Symbol = first.Stock?.Symbol ?? string.Empty,
                CompanyName = first.Stock?.CompanyName ?? string.Empty,
                LogoUrl = ToPublicLogo(first.Stock?.StockDetail?.LogoUrl),
                StockCode = $"{first.Exchange}:{first.Stock?.Symbol}",
                Exchange = first.Exchange,
                CurrentQuantity = remainingQty,
                LifetimeBoughtQuantity = boughtQty,
                LifetimeSoldQuantity = soldQty,
                AverageBuyPrice = Math.Round(avgBuy, 4),
                LifetimeAverageBuyPrice = Math.Round(lifetimeAvg, 4),
                AverageSellPrice = avgSell.HasValue ? Math.Round(avgSell.Value, 4) : null,
                MarketPrice = currentPrice,
                TotalInvestment = Math.Round(remainingInvestment, 2),
                LifetimeInvestment = Math.Round(lifetimeInvestment, 2),
                TotalCurrentValue = Math.Round(currentValue, 2),
                TotalOnSell = Math.Round(sellAmount, 2),
                UnrealizedGainLoss = Math.Round(unrealized, 2),
                UnrealizedGainLossPercent = remainingInvestment == 0
                    ? 0
                    : Math.Round((unrealized / remainingInvestment) * 100, 2),
                RealizedGainLoss = Math.Round(realized, 2),
                RealizedGainLossPercent = soldCost == 0
                    ? 0
                    : Math.Round((realized / soldCost) * 100, 2),
                TotalDividends = Math.Round(dividendTotal, 2),
                TotalGainLoss = Math.Round(totalPnl, 2),
                GainLossPercent = costBase == 0 ? 0 : Math.Round((totalPnl / costBase) * 100, 2),
                ProfitLoss = totalPnl > 0 ? ProfitLoss.Profit : totalPnl < 0 ? ProfitLoss.Loss : ProfitLoss.Equal,
                HoldDays = days,
                Status = status,
                CurrentType = status == PositionStatus.Holding ? InvestmentAction.Hold : InvestmentAction.Sell,
                BuyLotCount = holds.Count,
                OpenLotCount = holds.Count(h => h.RemainingQuantity > 0),
                SellCount = solds.Count,
                DividendCount = dividends.Count,
                FirstPurchaseDate = firstBuy,
                LastExitDate = lastExit,
                LastActivityDate = lastActivity,
                AsOfDate = today
            };
        }

        private static void RecalcHoldFromSells(PortfolioStockHoldEntity hold)
        {
            var soldQty = hold.PortfolioStockSolds.Sum(s => s.SellQuantity);
            if (soldQty > hold.Quantity)
            {
                throw new ValidationException(
                    $"Sold quantity {soldQty} exceeds lot quantity {hold.Quantity}.");
            }

            hold.RemainingQuantity = hold.Quantity - soldQty;
            hold.InvestmentAmount = hold.Quantity * hold.PurchasePrice;

            if (hold.RemainingQuantity <= 0)
            {
                hold.RemainingQuantity = 0;
                hold.IsSold = true;
                hold.LotStatus = LotStatus.FullySold;
                hold.ExitDate = hold.PortfolioStockSolds.Count > 0
                    ? hold.PortfolioStockSolds.Max(s => s.SoldDate.Date)
                    : hold.ExitDate;
                hold.HoldDays = hold.ExitDate.HasValue
                    ? ComputeHoldDays(hold.PurchaseDate, hold.ExitDate)
                    : hold.HoldDays;
            }
            else if (soldQty > 0)
            {
                hold.IsSold = false;
                hold.LotStatus = LotStatus.PartiallySold;
                hold.ExitDate = null;
                var lastSell = hold.PortfolioStockSolds.Max(s => s.SoldDate);
                hold.HoldDays = ComputeHoldDays(hold.PurchaseDate, lastSell);
            }
            else
            {
                hold.IsSold = false;
                hold.LotStatus = LotStatus.Open;
                hold.ExitDate = null;
                hold.HoldDays = null;
            }

            foreach (var sold in hold.PortfolioStockSolds)
            {
                sold.HoldDays = ComputeHoldDays(hold.PurchaseDate, sold.SoldDate);
                sold.LotStatus = hold.RemainingQuantity <= 0
                    ? LotStatus.FullySold
                    : LotStatus.PartiallySold;
            }
        }

        private static DateTime MaxDate(DateTime a, DateTime b) => a >= b ? a : b;

        private static long ComputeHoldDays(DateTime from, DateTime? to = null)
        {
            var days = ((to ?? DateTime.UtcNow).Date - from.Date).Days;
            return days < 0 ? 0 : days;
        }

        private static List<PortfolioDividendYearTotalResponse> GroupDividendsByYear(
            IEnumerable<PortfolioStockDividendEntity> dividends)
        {
            return dividends
                .GroupBy(d => d.DividendDate.Year)
                .OrderByDescending(g => g.Key)
                .Select(g => new PortfolioDividendYearTotalResponse
                {
                    Year = g.Key,
                    Amount = Math.Round(g.Sum(x => x.Amount), 2),
                    Count = g.Count()
                })
                .ToList();
        }
    }
}
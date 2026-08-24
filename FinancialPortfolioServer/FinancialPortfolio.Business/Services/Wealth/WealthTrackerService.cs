using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Abstractions.IValidation;
using FinancialPortfolio.Business.Abstractions.IWealth;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Data.Entities.Portfolio;
using FinancialPortfolio.Models.Abstractions.ICurrentUser;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Exceptions;
using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Request.Wealth;
using FinancialPortfolio.Models.Model.Response.Wealth;
using Microsoft.EntityFrameworkCore;

namespace FinancialPortfolio.Business.Services.Wealth
{
    public sealed class WealthTrackerService : IWealthTrackerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IApplicationLoggerService _logger;
        private readonly IValidationService _validation;
        private readonly ICurrentUserService _currentUser;

        public WealthTrackerService(
            ApplicationDbContext context,
            IApplicationLoggerService logger,
            IValidationService validation,
            ICurrentUserService currentUser)
        {
            _context = Guard.AgainstNull(context, nameof(context));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _validation = Guard.AgainstNull(validation, nameof(validation));
            _currentUser = Guard.AgainstNull(currentUser, nameof(currentUser));
        }

        public async Task<ApiResponse<WealthSummaryResponse>> GetSummaryAsync(CancellationToken cancellationToken)
        {
            var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
            if (portfolio is null)
                return ResponseFactory.Success(new WealthSummaryResponse(), "No portfolio created yet.");

            var funds = await MapFundsAsync(portfolio.Id, cancellationToken);
            var fds = await MapFdsAsync(portfolio.Id, cancellationToken);
            var rds = await MapRdsAsync(portfolio.Id, cancellationToken);
            var equity = await MapEquityAsync(portfolio.Id, cancellationToken);

            var buckets = new List<WealthBucketResponse> { equity, Bucket("mf", "Mutual funds", funds.Sum(x => x.InvestedAmount), funds.Sum(x => x.CurrentValue), funds.Count), Bucket("fd", "Fixed deposits", fds.Sum(x => x.Principal), fds.Sum(x => x.CurrentValue), fds.Count), Bucket("rd", "Recurring deposits", rds.Sum(x => x.InvestedAmount), rds.Sum(x => x.CurrentValue), rds.Count) };
            var invested = buckets.Sum(b => b.Invested);
            var value = buckets.Sum(b => b.CurrentValue);
            foreach (var b in buckets)
                b.AllocationPercent = value > 0 ? Math.Round(b.CurrentValue / value * 100m, 2) : 0;

            return ResponseFactory.Success(new WealthSummaryResponse
            {
                TotalInvested = Math.Round(invested, 2),
                TotalCurrentValue = Math.Round(value, 2),
                TotalGainLoss = Math.Round(value - invested, 2),
                TotalGainLossPercent = invested > 0 ? Math.Round((value - invested) / invested * 100m, 2) : 0,
                Buckets = buckets,
                MutualFunds = funds,
                FixedDeposits = fds,
                RecurringDeposits = rds
            }, "Wealth summary fetched.");
        }

        public async Task<ApiResponse<List<MutualFundResponse>>> GetMutualFundsAsync(CancellationToken cancellationToken)
        {
            var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
            if (portfolio is null) return ResponseFactory.Success(new List<MutualFundResponse>(), "No portfolio created yet.");
            return ResponseFactory.Success(await MapFundsAsync(portfolio.Id, cancellationToken), "Mutual funds fetched.");
        }

        public async Task<ApiResponse<MutualFundResponse>> AddMutualFundAsync(UpsertMutualFundRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var entity = ApplyFund(new PortfolioMutualFundEntity { PortfolioId = portfolio.Id }, request);
            await _context.PortfolioMutualFunds.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return ResponseFactory.Success(MapFund(entity), "Mutual fund added.");
        }

        public async Task<ApiResponse<MutualFundResponse>> UpdateMutualFundAsync(long id, UpsertMutualFundRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var entity = await _context.PortfolioMutualFunds.FirstOrDefaultAsync(x => x.Id == id && x.PortfolioId == portfolio.Id, cancellationToken)
                ?? throw new NotFoundException("Mutual fund not found.");
            ApplyFund(entity, request);
            await _context.SaveChangesAsync(cancellationToken);
            return ResponseFactory.Success(MapFund(entity), "Mutual fund updated.");
        }

        public async Task<ApiResponse<bool>> DeleteMutualFundAsync(long id, CancellationToken cancellationToken)
        {
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var entity = await _context.PortfolioMutualFunds.FirstOrDefaultAsync(x => x.Id == id && x.PortfolioId == portfolio.Id, cancellationToken)
                ?? throw new NotFoundException("Mutual fund not found.");
            _context.PortfolioMutualFunds.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return ResponseFactory.Success(true, "Mutual fund deleted.");
        }

        public async Task<ApiResponse<List<FixedDepositResponse>>> GetFixedDepositsAsync(CancellationToken cancellationToken)
        {
            var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
            if (portfolio is null) return ResponseFactory.Success(new List<FixedDepositResponse>(), "No portfolio created yet.");
            return ResponseFactory.Success(await MapFdsAsync(portfolio.Id, cancellationToken), "Fixed deposits fetched.");
        }

        public async Task<ApiResponse<FixedDepositResponse>> AddFixedDepositAsync(UpsertFixedDepositRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var entity = ApplyFd(new PortfolioFixedDepositEntity { PortfolioId = portfolio.Id }, request);
            await _context.PortfolioFixedDeposits.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return ResponseFactory.Success(MapFd(entity), "Fixed deposit added.");
        }

        public async Task<ApiResponse<FixedDepositResponse>> UpdateFixedDepositAsync(long id, UpsertFixedDepositRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var entity = await _context.PortfolioFixedDeposits.FirstOrDefaultAsync(x => x.Id == id && x.PortfolioId == portfolio.Id, cancellationToken)
                ?? throw new NotFoundException("Fixed deposit not found.");
            ApplyFd(entity, request);
            await _context.SaveChangesAsync(cancellationToken);
            return ResponseFactory.Success(MapFd(entity), "Fixed deposit updated.");
        }

        public async Task<ApiResponse<bool>> DeleteFixedDepositAsync(long id, CancellationToken cancellationToken)
        {
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var entity = await _context.PortfolioFixedDeposits.FirstOrDefaultAsync(x => x.Id == id && x.PortfolioId == portfolio.Id, cancellationToken)
                ?? throw new NotFoundException("Fixed deposit not found.");
            _context.PortfolioFixedDeposits.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return ResponseFactory.Success(true, "Fixed deposit deleted.");
        }

        public async Task<ApiResponse<List<RecurringDepositResponse>>> GetRecurringDepositsAsync(CancellationToken cancellationToken)
        {
            var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
            if (portfolio is null) return ResponseFactory.Success(new List<RecurringDepositResponse>(), "No portfolio created yet.");
            return ResponseFactory.Success(await MapRdsAsync(portfolio.Id, cancellationToken), "Recurring deposits fetched.");
        }

        public async Task<ApiResponse<RecurringDepositResponse>> AddRecurringDepositAsync(UpsertRecurringDepositRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var entity = ApplyRd(new PortfolioRecurringDepositEntity { PortfolioId = portfolio.Id }, request);
            await _context.PortfolioRecurringDeposits.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return ResponseFactory.Success(MapRd(entity), "Recurring deposit added.");
        }

        public async Task<ApiResponse<RecurringDepositResponse>> UpdateRecurringDepositAsync(long id, UpsertRecurringDepositRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var entity = await _context.PortfolioRecurringDeposits.FirstOrDefaultAsync(x => x.Id == id && x.PortfolioId == portfolio.Id, cancellationToken)
                ?? throw new NotFoundException("Recurring deposit not found.");
            ApplyRd(entity, request);
            await _context.SaveChangesAsync(cancellationToken);
            return ResponseFactory.Success(MapRd(entity), "Recurring deposit updated.");
        }

        public async Task<ApiResponse<bool>> DeleteRecurringDepositAsync(long id, CancellationToken cancellationToken)
        {
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var entity = await _context.PortfolioRecurringDeposits.FirstOrDefaultAsync(x => x.Id == id && x.PortfolioId == portfolio.Id, cancellationToken)
                ?? throw new NotFoundException("Recurring deposit not found.");
            _context.PortfolioRecurringDeposits.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return ResponseFactory.Success(true, "Recurring deposit deleted.");
        }

        private async Task<PortfolioEntity?> GetPortfolioOrNullAsync(CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;
            return await _context.Portfolios.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive, cancellationToken);
        }

        private async Task<PortfolioEntity> RequirePortfolioAsync(CancellationToken cancellationToken)
        {
            var tracked = await _context.Portfolios.FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId && p.IsActive, cancellationToken);
            return tracked ?? throw new NotFoundException("Create a portfolio first.");
        }

        private async Task<WealthBucketResponse> MapEquityAsync(long portfolioId, CancellationToken cancellationToken)
        {
            var holds = await _context.PortfolioStockHolds
                .AsNoTracking()
                .Include(h => h.Stock).ThenInclude(s => s.StockDetail)
                .Where(h => h.PortfolioId == portfolioId && h.RemainingQuantity > 0)
                .ToListAsync(cancellationToken);

            var invested = holds.Sum(h => h.RemainingQuantity * h.PurchasePrice);
            var value = holds.Sum(h => h.RemainingQuantity * (h.Stock.StockDetail?.CurrentPrice ?? h.PurchasePrice));
            var count = holds.Select(h => h.StockId).Distinct().Count();
            return Bucket("equity", "Stocks", invested, value, count);
        }

        private async Task<List<MutualFundResponse>> MapFundsAsync(long portfolioId, CancellationToken cancellationToken)
        {
            var rows = await _context.PortfolioMutualFunds.AsNoTracking()
                .Where(x => x.PortfolioId == portfolioId)
                .OrderBy(x => x.SchemeName)
                .ToListAsync(cancellationToken);
            return rows.Select(MapFund).ToList();
        }

        private async Task<List<FixedDepositResponse>> MapFdsAsync(long portfolioId, CancellationToken cancellationToken)
        {
            var rows = await _context.PortfolioFixedDeposits.AsNoTracking()
                .Where(x => x.PortfolioId == portfolioId)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);
            return rows.Select(MapFd).ToList();
        }

        private async Task<List<RecurringDepositResponse>> MapRdsAsync(long portfolioId, CancellationToken cancellationToken)
        {
            var rows = await _context.PortfolioRecurringDeposits.AsNoTracking()
                .Where(x => x.PortfolioId == portfolioId)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);
            return rows.Select(MapRd).ToList();
        }

        private static WealthBucketResponse Bucket(string key, string label, decimal invested, decimal value, int count)
            => new()
            {
                Key = key,
                Label = label,
                Invested = Math.Round(invested, 2),
                CurrentValue = Math.Round(value, 2),
                GainLoss = Math.Round(value - invested, 2),
                Count = count
            };

        private static PortfolioMutualFundEntity ApplyFund(PortfolioMutualFundEntity entity, UpsertMutualFundRequest request)
        {
            entity.SchemeName = request.SchemeName.Trim();
            entity.Amc = request.Amc.Trim();
            entity.FolioNumber = string.IsNullOrWhiteSpace(request.FolioNumber) ? null : request.FolioNumber.Trim();
            entity.SchemeCode = request.SchemeCode;
            entity.SchemeType = request.SchemeType;
            entity.Units = request.Units;
            entity.AverageNav = request.AverageNav;
            entity.CurrentNav = request.CurrentNav > 0 ? request.CurrentNav : request.AverageNav;
            entity.InvestedAmount = DepositMath.Round(request.Units * request.AverageNav);
            entity.PurchaseDate = request.PurchaseDate.Date;
            entity.Notes = request.Notes;
            entity.IsActive = request.IsActive;
            return entity;
        }

        private static PortfolioFixedDepositEntity ApplyFd(PortfolioFixedDepositEntity entity, UpsertFixedDepositRequest request)
        {
            entity.BankName = request.BankName.Trim();
            entity.AccountRef = string.IsNullOrWhiteSpace(request.AccountRef) ? null : request.AccountRef.Trim();
            entity.Principal = request.Principal;
            entity.InterestRate = request.InterestRate;
            entity.TenureMonths = request.TenureMonths;
            entity.InterestType = request.InterestType;
            entity.StartDate = request.StartDate.Date;
            entity.MaturityDate = DepositMath.MaturityDate(request.StartDate, request.TenureMonths);
            entity.Status = request.Status == 0 ? DepositStatus.Active : request.Status;
            entity.Notes = request.Notes;
            if (entity.Status == DepositStatus.Active && DateTime.UtcNow.Date >= entity.MaturityDate)
                entity.Status = DepositStatus.Matured;
            return entity;
        }

        private static PortfolioRecurringDepositEntity ApplyRd(PortfolioRecurringDepositEntity entity, UpsertRecurringDepositRequest request)
        {
            entity.BankName = request.BankName.Trim();
            entity.AccountRef = string.IsNullOrWhiteSpace(request.AccountRef) ? null : request.AccountRef.Trim();
            entity.MonthlyAmount = request.MonthlyAmount;
            entity.InterestRate = request.InterestRate;
            entity.TenureMonths = request.TenureMonths;
            entity.InstallmentsPaid = Math.Clamp(request.InstallmentsPaid, 0, request.TenureMonths);
            entity.StartDate = request.StartDate.Date;
            entity.MaturityDate = DepositMath.MaturityDate(request.StartDate, request.TenureMonths);
            entity.Status = request.Status == 0 ? DepositStatus.Active : request.Status;
            entity.Notes = request.Notes;
            if (entity.Status == DepositStatus.Active && DateTime.UtcNow.Date >= entity.MaturityDate)
                entity.Status = DepositStatus.Matured;
            return entity;
        }

        private static MutualFundResponse MapFund(PortfolioMutualFundEntity x)
        {
            var current = DepositMath.Round(x.Units * x.CurrentNav);
            var gain = current - x.InvestedAmount;
            return new MutualFundResponse
            {
                Id = x.Id,
                SchemeName = x.SchemeName,
                Amc = x.Amc,
                FolioNumber = x.FolioNumber,
                SchemeCode = x.SchemeCode,
                NavAsOf = x.NavAsOf,
                NavSource = x.NavSource,
                SchemeType = x.SchemeType,
                Units = x.Units,
                AverageNav = x.AverageNav,
                CurrentNav = x.CurrentNav,
                InvestedAmount = x.InvestedAmount,
                CurrentValue = current,
                GainLoss = gain,
                GainLossPercent = x.InvestedAmount > 0 ? Math.Round(gain / x.InvestedAmount * 100m, 2) : 0,
                PurchaseDate = x.PurchaseDate,
                Notes = x.Notes,
                IsActive = x.IsActive
            };
        }

        private static FixedDepositResponse MapFd(PortfolioFixedDepositEntity x)
        {
            var now = DateTime.UtcNow.Date;
            var maturityAmt = DepositMath.FdMaturity(x.Principal, x.InterestRate, x.TenureMonths, x.InterestType);
            var current = DepositMath.FdCurrent(x.Principal, x.InterestRate, x.TenureMonths, x.StartDate, x.MaturityDate, now, x.InterestType);
            return new FixedDepositResponse
            {
                Id = x.Id,
                BankName = x.BankName,
                AccountRef = x.AccountRef,
                Principal = x.Principal,
                InterestRate = x.InterestRate,
                TenureMonths = x.TenureMonths,
                InterestType = x.InterestType,
                StartDate = x.StartDate,
                MaturityDate = x.MaturityDate,
                MaturityAmount = maturityAmt,
                CurrentValue = current,
                AccruedInterest = DepositMath.Round(current - x.Principal),
                Status = x.Status == DepositStatus.Active && now >= x.MaturityDate ? DepositStatus.Matured : x.Status,
                Notes = x.Notes
            };
        }

        private static RecurringDepositResponse MapRd(PortfolioRecurringDepositEntity x)
        {
            var now = DateTime.UtcNow.Date;
            var paid = DepositMath.ElapsedInstallments(x.StartDate, x.TenureMonths, x.InstallmentsPaid, now);
            var invested = DepositMath.Round(x.MonthlyAmount * paid);
            return new RecurringDepositResponse
            {
                Id = x.Id,
                BankName = x.BankName,
                AccountRef = x.AccountRef,
                MonthlyAmount = x.MonthlyAmount,
                InterestRate = x.InterestRate,
                TenureMonths = x.TenureMonths,
                InstallmentsPaid = paid,
                StartDate = x.StartDate,
                MaturityDate = x.MaturityDate,
                InvestedAmount = invested,
                CurrentValue = DepositMath.RdCurrent(x.MonthlyAmount, x.InterestRate, x.TenureMonths, paid, x.StartDate, now),
                MaturityAmount = DepositMath.RdMaturity(x.MonthlyAmount, x.InterestRate, x.TenureMonths),
                Status = x.Status == DepositStatus.Active && now >= x.MaturityDate ? DepositStatus.Matured : x.Status,
                Notes = x.Notes
            };
        }
    }
}

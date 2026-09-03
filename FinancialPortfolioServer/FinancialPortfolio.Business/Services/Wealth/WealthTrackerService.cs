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
            var policies = await MapPoliciesAsync(portfolio.Id, cancellationToken);
            var (equity, etfs) = await MapEquityBucketsAsync(portfolio.Id, cancellationToken);

            var buckets = new List<WealthBucketResponse>
            {
                equity,
                etfs,
                Bucket("mf", "Mutual funds", funds.Sum(x => x.InvestedAmount), funds.Sum(x => x.CurrentValue), funds.Count),
                Bucket("fd", "Fixed deposits", fds.Sum(x => x.Principal), fds.Sum(x => x.CurrentValue), fds.Count),
                Bucket("rd", "Recurring deposits", rds.Sum(x => x.InvestedAmount), rds.Sum(x => x.CurrentValue), rds.Count),
                Bucket("insurance", "Insurance policies", policies.Sum(x => x.TotalPremiumsPaid), policies.Sum(x => x.CurrentValue), policies.Count)
            };
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
                RecurringDeposits = rds,
                InsurancePolicies = policies
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

        public async Task<ApiResponse<RecurringDepositResponse>> GetRecurringDepositDetailAsync(long id, CancellationToken cancellationToken)
        {
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var entity = await _context.PortfolioRecurringDeposits
                .AsNoTracking()
                .Include(x => x.Installments)
                .FirstOrDefaultAsync(x => x.Id == id && x.PortfolioId == portfolio.Id, cancellationToken)
                ?? throw new NotFoundException("Recurring deposit not found.");

            var response = MapRd(entity);
            response.Installments = entity.Installments
                .OrderBy(i => i.InstallmentNumber)
                .Select(MapInstallment)
                .ToList();

            return ResponseFactory.Success(response, "RD detail fetched.");
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

        public async Task<ApiResponse<List<RdInstallmentResponse>>> GetRdInstallmentsAsync(long rdId, CancellationToken cancellationToken)
        {
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var exists = await _context.PortfolioRecurringDeposits
                .AnyAsync(x => x.Id == rdId && x.PortfolioId == portfolio.Id, cancellationToken);
            if (!exists)
                throw new NotFoundException("Recurring deposit not found.");

            var rows = await _context.PortfolioRecurringDepositInstallments
                .AsNoTracking()
                .Where(x => x.RecurringDepositId == rdId)
                .OrderBy(x => x.InstallmentNumber)
                .ToListAsync(cancellationToken);

            return ResponseFactory.Success(rows.Select(MapInstallment).ToList(), "Installments fetched.");
        }

        public async Task<ApiResponse<RdInstallmentResponse>> PayRdInstallmentAsync(
            long rdId,
            PayRdInstallmentRequest request,
            CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var rd = await _context.PortfolioRecurringDeposits
                .Include(x => x.Installments)
                .FirstOrDefaultAsync(x => x.Id == rdId && x.PortfolioId == portfolio.Id, cancellationToken)
                ?? throw new NotFoundException("Recurring deposit not found.");

            if (rd.Status != DepositStatus.Active)
                throw new ValidationException("Only active RDs can accept installment payments.");

            var nextNumber = request.InstallmentNumber
                ?? (rd.Installments.Count > 0
                    ? rd.Installments.Max(i => i.InstallmentNumber) + 1
                    : rd.InstallmentsPaid + 1);

            if (nextNumber < 1 || nextNumber > rd.TenureMonths)
                throw new ValidationException($"Installment number must be between 1 and {rd.TenureMonths}.");

            var dueDate = rd.StartDate.Date.AddMonths(nextNumber - 1);
            var today = DateTime.UtcNow.Date;

            // Block future installments (due date not yet reached)
            if (dueDate > today)
                throw new ValidationException(
                    $"Cannot record installment #{nextNumber}. Its due date ({dueDate:dd MMM yyyy}) is in the future.");

            var existing = rd.Installments.FirstOrDefault(i => i.InstallmentNumber == nextNumber);
            if (existing is not null && existing.Status == RdInstallmentStatus.Paid)
                throw new ValidationException($"Installment #{nextNumber} is already paid.");

            var paidDate = (request.PaidDate ?? DateTime.UtcNow).Date;
            if (paidDate > today)
                throw new ValidationException("Paid date cannot be in the future.");

            var amount = request.Amount is > 0 ? request.Amount.Value : rd.MonthlyAmount;

            PortfolioRecurringDepositInstallmentEntity row;
            if (existing is null)
            {
                row = new PortfolioRecurringDepositInstallmentEntity
                {
                    RecurringDepositId = rd.Id,
                    InstallmentNumber = nextNumber,
                    DueDate = dueDate,
                    PaidDate = paidDate,
                    Amount = amount,
                    FromBankName = string.IsNullOrWhiteSpace(request.FromBankName) ? null : request.FromBankName.Trim(),
                    FromAccountNumber = string.IsNullOrWhiteSpace(request.FromAccountNumber) ? null : request.FromAccountNumber.Trim(),
                    FromIfsc = string.IsNullOrWhiteSpace(request.FromIfsc) ? null : request.FromIfsc.Trim().ToUpperInvariant(),
                    TransactionReference = string.IsNullOrWhiteSpace(request.TransactionReference) ? null : request.TransactionReference.Trim(),
                    PaymentMode = request.PaymentMode,
                    Status = RdInstallmentStatus.Paid,
                    PenaltyAmount = request.PenaltyAmount,
                    Notes = request.Notes
                };
                await _context.PortfolioRecurringDepositInstallments.AddAsync(row, cancellationToken);
            }
            else
            {
                row = existing;
                row.PaidDate = paidDate;
                row.Amount = amount;
                row.FromBankName = string.IsNullOrWhiteSpace(request.FromBankName) ? row.FromBankName : request.FromBankName.Trim();
                row.FromAccountNumber = string.IsNullOrWhiteSpace(request.FromAccountNumber) ? row.FromAccountNumber : request.FromAccountNumber.Trim();
                row.FromIfsc = string.IsNullOrWhiteSpace(request.FromIfsc) ? row.FromIfsc : request.FromIfsc.Trim().ToUpperInvariant();
                row.TransactionReference = string.IsNullOrWhiteSpace(request.TransactionReference) ? row.TransactionReference : request.TransactionReference.Trim();
                row.PaymentMode = request.PaymentMode;
                row.Status = RdInstallmentStatus.Paid;
                row.PenaltyAmount = request.PenaltyAmount ?? row.PenaltyAmount;
                row.Notes = request.Notes ?? row.Notes;
            }

            await _context.SaveChangesAsync(cancellationToken);
            await SyncRdPaidCountAsync(rd, cancellationToken);

            return ResponseFactory.Success(MapInstallment(row), $"Installment #{nextNumber} recorded.");
        }

        public async Task<ApiResponse<RdInstallmentResponse>> UpsertRdInstallmentAsync(
            long rdId,
            UpsertRdInstallmentRequest request,
            CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var rd = await _context.PortfolioRecurringDeposits
                .Include(x => x.Installments)
                .FirstOrDefaultAsync(x => x.Id == rdId && x.PortfolioId == portfolio.Id, cancellationToken)
                ?? throw new NotFoundException("Recurring deposit not found.");

            if (request.InstallmentNumber < 1 || request.InstallmentNumber > rd.TenureMonths)
                throw new ValidationException($"Installment number must be between 1 and {rd.TenureMonths}.");

            var today = DateTime.UtcNow.Date;
            var dueDate = request.DueDate.Date;

            // Block future installments
            if (dueDate > today)
                throw new ValidationException(
                    $"Cannot save installment #{request.InstallmentNumber}. Due date ({dueDate:dd MMM yyyy}) is in the future.");

            if (request.PaidDate is DateTime pd && pd.Date > today)
                throw new ValidationException("Paid date cannot be in the future.");

            var row = rd.Installments.FirstOrDefault(i => i.InstallmentNumber == request.InstallmentNumber);
            if (row is null)
            {
                row = new PortfolioRecurringDepositInstallmentEntity
                {
                    RecurringDepositId = rd.Id,
                    InstallmentNumber = request.InstallmentNumber
                };
                await _context.PortfolioRecurringDepositInstallments.AddAsync(row, cancellationToken);
            }

            row.DueDate = request.DueDate.Date;
            row.PaidDate = request.PaidDate?.Date;
            row.Amount = request.Amount;
            row.FromBankName = string.IsNullOrWhiteSpace(request.FromBankName) ? null : request.FromBankName.Trim();
            row.FromAccountNumber = string.IsNullOrWhiteSpace(request.FromAccountNumber) ? null : request.FromAccountNumber.Trim();
            row.FromIfsc = string.IsNullOrWhiteSpace(request.FromIfsc) ? null : request.FromIfsc.Trim().ToUpperInvariant();
            row.TransactionReference = string.IsNullOrWhiteSpace(request.TransactionReference) ? null : request.TransactionReference.Trim();
            row.PaymentMode = request.PaymentMode;
            row.Status = request.Status;
            row.PenaltyAmount = request.PenaltyAmount;
            row.Notes = request.Notes;

            await _context.SaveChangesAsync(cancellationToken);
            await SyncRdPaidCountAsync(rd, cancellationToken);

            return ResponseFactory.Success(MapInstallment(row), "Installment saved.");
        }

        public async Task<ApiResponse<bool>> DeleteRdInstallmentAsync(long rdId, long installmentId, CancellationToken cancellationToken)
        {
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var rd = await _context.PortfolioRecurringDeposits
                .FirstOrDefaultAsync(x => x.Id == rdId && x.PortfolioId == portfolio.Id, cancellationToken)
                ?? throw new NotFoundException("Recurring deposit not found.");

            var row = await _context.PortfolioRecurringDepositInstallments
                .FirstOrDefaultAsync(x => x.Id == installmentId && x.RecurringDepositId == rdId, cancellationToken)
                ?? throw new NotFoundException("Installment not found.");

            _context.PortfolioRecurringDepositInstallments.Remove(row);
            await _context.SaveChangesAsync(cancellationToken);
            await SyncRdPaidCountAsync(rd, cancellationToken);

            return ResponseFactory.Success(true, "Installment deleted.");
        }

        public async Task<ApiResponse<List<InsurancePolicyResponse>>> GetInsurancePoliciesAsync(CancellationToken cancellationToken)
        {
            var portfolio = await GetPortfolioOrNullAsync(cancellationToken);
            if (portfolio is null) return ResponseFactory.Success(new List<InsurancePolicyResponse>(), "No portfolio created yet.");
            return ResponseFactory.Success(await MapPoliciesAsync(portfolio.Id, cancellationToken), "Insurance policies fetched.");
        }

        public async Task<ApiResponse<InsurancePolicyResponse>> AddInsurancePolicyAsync(UpsertInsurancePolicyRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var policyNumber = request.PolicyNumber.Trim();
            var duplicate = await _context.PortfolioInsurancePolicies.AsNoTracking()
                .AnyAsync(x => x.PortfolioId == portfolio.Id && x.PolicyNumber == policyNumber, cancellationToken);
            if (duplicate)
                throw new ConflictException($"A policy with number '{policyNumber}' already exists in this portfolio.");
            var entity = ApplyPolicy(new PortfolioInsurancePolicyEntity { PortfolioId = portfolio.Id }, request);
            await _context.PortfolioInsurancePolicies.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return ResponseFactory.Success(MapPolicy(entity), "Insurance policy added.");
        }

        public async Task<ApiResponse<InsurancePolicyResponse>> UpdateInsurancePolicyAsync(long id, UpsertInsurancePolicyRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var entity = await _context.PortfolioInsurancePolicies.FirstOrDefaultAsync(x => x.Id == id && x.PortfolioId == portfolio.Id, cancellationToken)
                ?? throw new NotFoundException("Insurance policy not found.");
            var policyNumber = request.PolicyNumber.Trim();
            var duplicate = await _context.PortfolioInsurancePolicies.AsNoTracking()
                .AnyAsync(x => x.PortfolioId == portfolio.Id && x.PolicyNumber == policyNumber && x.Id != id, cancellationToken);
            if (duplicate)
                throw new ConflictException($"A policy with number '{policyNumber}' already exists in this portfolio.");
            ApplyPolicy(entity, request);
            await _context.SaveChangesAsync(cancellationToken);
            return ResponseFactory.Success(MapPolicy(entity), "Insurance policy updated.");
        }

        public async Task<ApiResponse<bool>> DeleteInsurancePolicyAsync(long id, CancellationToken cancellationToken)
        {
            var portfolio = await RequirePortfolioAsync(cancellationToken);
            var entity = await _context.PortfolioInsurancePolicies.FirstOrDefaultAsync(x => x.Id == id && x.PortfolioId == portfolio.Id, cancellationToken)
                ?? throw new NotFoundException("Insurance policy not found.");
            _context.PortfolioInsurancePolicies.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return ResponseFactory.Success(true, "Insurance policy deleted.");
        }

        private async Task SyncRdPaidCountAsync(PortfolioRecurringDepositEntity rd, CancellationToken cancellationToken)
        {
            var paidCount = await _context.PortfolioRecurringDepositInstallments
                .CountAsync(i => i.RecurringDepositId == rd.Id && i.Status == RdInstallmentStatus.Paid, cancellationToken);

            rd.InstallmentsPaid = Math.Clamp(paidCount, 0, rd.TenureMonths);

            if (rd.InstallmentsPaid >= rd.TenureMonths)
                rd.Status = DepositStatus.Matured;
            else if (rd.Status == DepositStatus.Matured)
                rd.Status = DepositStatus.Active;

            await _context.SaveChangesAsync(cancellationToken);
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

        private async Task<(WealthBucketResponse Stocks, WealthBucketResponse Etfs)> MapEquityBucketsAsync(long portfolioId, CancellationToken cancellationToken)
        {
            var holds = await _context.PortfolioStockHolds
                .AsNoTracking()
                .Include(h => h.Stock).ThenInclude(s => s.StockDetail)
                .Where(h => h.PortfolioId == portfolioId && h.RemainingQuantity > 0)
                .ToListAsync(cancellationToken);

            var etfHolds = holds.Where(IsEtfHolding).ToList();
            var stockHolds = holds.Where(h => !IsEtfHolding(h)).ToList();

            return (
                BucketFromHolds("equity", "Stocks", stockHolds),
                BucketFromHolds("etf", "ETFs", etfHolds)
            );
        }

        private static bool IsEtfHolding(PortfolioStockHoldEntity hold)
        {
            var series = hold.Stock?.Series ?? string.Empty;
            var category = hold.Stock?.StockDetail?.Category ?? string.Empty;
            var company = hold.Stock?.CompanyName ?? string.Empty;
            var symbol = hold.Stock?.Symbol ?? string.Empty;

            static bool HasEtf(string value) =>
                value.Contains("ETF", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Exchange Traded", StringComparison.OrdinalIgnoreCase);

            return HasEtf(series) || HasEtf(category) || HasEtf(company) || HasEtf(symbol);
        }

        private static WealthBucketResponse BucketFromHolds(string key, string label, List<PortfolioStockHoldEntity> holds)
        {
            var invested = holds.Sum(h => h.RemainingQuantity * h.PurchasePrice);
            var value = holds.Sum(h => h.RemainingQuantity * (h.Stock.StockDetail?.CurrentPrice ?? h.PurchasePrice));
            var count = holds.Select(h => h.StockId).Distinct().Count();
            return Bucket(key, label, invested, value, count);
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
            entity.BankIfsc = string.IsNullOrWhiteSpace(request.BankIfsc) ? null : request.BankIfsc.Trim().ToUpperInvariant();
            entity.AccountRef = string.IsNullOrWhiteSpace(request.AccountRef) ? null : request.AccountRef.Trim();
            entity.LinkedAccountNumber = string.IsNullOrWhiteSpace(request.LinkedAccountNumber) ? null : request.LinkedAccountNumber.Trim();
            entity.LinkedIfsc = string.IsNullOrWhiteSpace(request.LinkedIfsc) ? null : request.LinkedIfsc.Trim().ToUpperInvariant();
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
            // Use stored paid count only — never auto-infer from calendar months.
            // Past RDs must be backfilled via Pay installment / Upsert installment.
            var paid = Math.Clamp(x.InstallmentsPaid, 0, x.TenureMonths);
            var invested = DepositMath.Round(x.MonthlyAmount * paid);
            return new RecurringDepositResponse
            {
                Id = x.Id,
                BankName = x.BankName,
                BankIfsc = x.BankIfsc,
                AccountRef = x.AccountRef,
                LinkedAccountNumber = x.LinkedAccountNumber,
                LinkedIfsc = x.LinkedIfsc,
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
                Notes = x.Notes,
                Installments = []
            };
        }

        private static RdInstallmentResponse MapInstallment(PortfolioRecurringDepositInstallmentEntity x) => new()
        {
            Id = x.Id,
            RecurringDepositId = x.RecurringDepositId,
            InstallmentNumber = x.InstallmentNumber,
            DueDate = x.DueDate,
            PaidDate = x.PaidDate,
            Amount = x.Amount,
            FromBankName = x.FromBankName,
            FromAccountNumber = x.FromAccountNumber,
            FromIfsc = x.FromIfsc,
            TransactionReference = x.TransactionReference,
            PaymentMode = x.PaymentMode,
            Status = x.Status,
            PenaltyAmount = x.PenaltyAmount,
            Notes = x.Notes
        };

        private async Task<List<InsurancePolicyResponse>> MapPoliciesAsync(long portfolioId, CancellationToken cancellationToken)
        {
            var rows = await _context.PortfolioInsurancePolicies.AsNoTracking()
                .Where(x => x.PortfolioId == portfolioId)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);
            return rows.Select(MapPolicy).ToList();
        }

        private static PortfolioInsurancePolicyEntity ApplyPolicy(PortfolioInsurancePolicyEntity entity, UpsertInsurancePolicyRequest request)
        {
            entity.InsurerName = request.InsurerName.Trim();
            entity.PolicyNumber = request.PolicyNumber.Trim();
            entity.PlanName = request.PlanName.Trim();
            entity.PolicyType = request.PolicyType;
            entity.SumAssured = request.SumAssured;
            entity.PremiumAmount = request.PremiumAmount;
            entity.PremiumFrequency = request.PremiumFrequency;
            entity.PremiumPayingTermYears = request.PremiumPayingTermYears;
            entity.PolicyTermYears = request.PolicyTermYears;
            entity.StartDate = request.StartDate.Date;
            entity.MaturityDate = InsuranceMath.MaturityDate(request.StartDate, request.PolicyTermYears);
            var max = InsuranceMath.MaxInstallments(request.PremiumFrequency, request.PremiumPayingTermYears);
            entity.PremiumsPaid = Math.Clamp(request.PremiumsPaid, 0, max > 0 ? max : request.PremiumsPaid);
            entity.ExpectedMaturityAmount = request.ExpectedMaturityAmount;
            entity.Status = request.Status == 0 ? InsurancePolicyStatus.Active : request.Status;
            entity.Notes = request.Notes;
            if (entity.Status == InsurancePolicyStatus.Active && DateTime.UtcNow.Date >= entity.MaturityDate)
                entity.Status = InsurancePolicyStatus.Matured;
            return entity;
        }

        private static InsurancePolicyResponse MapPolicy(PortfolioInsurancePolicyEntity x)
        {
            var now = DateTime.UtcNow.Date;
            var max = InsuranceMath.MaxInstallments(x.PremiumFrequency, x.PremiumPayingTermYears);
            var totalPaid = InsuranceMath.TotalPremiumsPaid(x.PremiumAmount, x.PremiumsPaid);
            var current = InsuranceMath.CurrentValue(x.PolicyType, totalPaid, x.ExpectedMaturityAmount, x.Status, x.MaturityDate, now);
            var status = x.Status == InsurancePolicyStatus.Active && now >= x.MaturityDate
                ? InsurancePolicyStatus.Matured
                : x.Status;
            return new InsurancePolicyResponse
            {
                Id = x.Id,
                InsurerName = x.InsurerName,
                PolicyNumber = x.PolicyNumber,
                PlanName = x.PlanName,
                PolicyType = x.PolicyType,
                SumAssured = x.SumAssured,
                PremiumAmount = x.PremiumAmount,
                PremiumFrequency = x.PremiumFrequency,
                PremiumPayingTermYears = x.PremiumPayingTermYears,
                PolicyTermYears = x.PolicyTermYears,
                StartDate = x.StartDate,
                MaturityDate = x.MaturityDate,
                PremiumsPaid = x.PremiumsPaid,
                MaxPremiumInstallments = max,
                TotalPremiumsPaid = totalPaid,
                ExpectedMaturityAmount = x.ExpectedMaturityAmount,
                CurrentValue = current,
                GainLoss = InsuranceMath.Round(current - totalPaid),
                Status = status,
                Notes = x.Notes
            };
        }
    }
}

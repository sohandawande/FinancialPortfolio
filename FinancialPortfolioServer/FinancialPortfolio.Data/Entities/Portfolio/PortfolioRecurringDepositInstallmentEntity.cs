using FinancialPortfolio.Data.Common.Base;
using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Data.Entities.Portfolio
{
    public sealed class PortfolioRecurringDepositInstallmentEntity : BaseEntity
    {
        public long Id { get; set; }
        public long RecurringDepositId { get; set; }
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public decimal Amount { get; set; }
        public string? FromBankName { get; set; }
        public string? FromAccountNumber { get; set; }
        public string? FromIfsc { get; set; }
        public string? TransactionReference { get; set; }
        public RdPaymentMode PaymentMode { get; set; } = RdPaymentMode.AutoDebit;
        public RdInstallmentStatus Status { get; set; } = RdInstallmentStatus.Pending;
        public decimal? PenaltyAmount { get; set; }
        public string? Notes { get; set; }

        public PortfolioRecurringDepositEntity RecurringDeposit { get; set; } = default!;
    }
}

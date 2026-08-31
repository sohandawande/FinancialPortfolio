using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Request.Wealth
{
    public class UpsertRdInstallmentRequest
    {
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public decimal Amount { get; set; }
        public string? FromBankName { get; set; }
        public string? FromAccountNumber { get; set; }
        public string? FromIfsc { get; set; }
        public string? TransactionReference { get; set; }
        public RdPaymentMode PaymentMode { get; set; } = RdPaymentMode.AutoDebit;
        public RdInstallmentStatus Status { get; set; } = RdInstallmentStatus.Paid;
        public decimal? PenaltyAmount { get; set; }
        public string? Notes { get; set; }
    }
}

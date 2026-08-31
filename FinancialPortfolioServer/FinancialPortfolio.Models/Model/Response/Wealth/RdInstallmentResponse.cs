using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Response.Wealth
{
    public class RdInstallmentResponse
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
        public RdPaymentMode PaymentMode { get; set; }
        public RdInstallmentStatus Status { get; set; }
        public decimal? PenaltyAmount { get; set; }
        public string? Notes { get; set; }
    }
}

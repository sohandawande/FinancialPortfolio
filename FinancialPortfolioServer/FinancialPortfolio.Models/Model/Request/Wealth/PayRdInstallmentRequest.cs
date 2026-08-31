using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Request.Wealth
{
    public class PayRdInstallmentRequest
    {
        public int? InstallmentNumber { get; set; }
        public DateTime? PaidDate { get; set; }
        public decimal? Amount { get; set; }
        public string? FromBankName { get; set; }
        public string? FromAccountNumber { get; set; }
        public string? FromIfsc { get; set; }
        public string? TransactionReference { get; set; }
        public RdPaymentMode PaymentMode { get; set; } = RdPaymentMode.AutoDebit;
        public decimal? PenaltyAmount { get; set; }
        public string? Notes { get; set; }
    }
}

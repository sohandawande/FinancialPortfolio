using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Response.Wealth
{
    public class RecurringDepositResponse
    {
        public long Id { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string? BankIfsc { get; set; }
        public string? AccountRef { get; set; }
        public string? LinkedAccountNumber { get; set; }
        public string? LinkedIfsc { get; set; }
        public decimal MonthlyAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public int InstallmentsPaid { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public decimal InvestedAmount { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal MaturityAmount { get; set; }
        public DepositStatus Status { get; set; }
        public string? Notes { get; set; }
        public List<RdInstallmentResponse> Installments { get; set; } = [];
    }
}

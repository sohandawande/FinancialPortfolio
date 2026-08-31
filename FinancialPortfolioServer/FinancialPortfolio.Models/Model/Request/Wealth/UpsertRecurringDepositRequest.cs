using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Request.Wealth
{
    public class UpsertRecurringDepositRequest
    {
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
        public string? Notes { get; set; }
        public DepositStatus Status { get; set; } = DepositStatus.Active;
    }
}

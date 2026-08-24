using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Response.Wealth
{
    public class FixedDepositResponse
    {
        public long Id { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string? AccountRef { get; set; }
        public decimal Principal { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public DepositInterestType InterestType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public decimal MaturityAmount { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal AccruedInterest { get; set; }
        public DepositStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}

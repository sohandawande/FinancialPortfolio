using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Request.Wealth
{
    public class UpsertFixedDepositRequest
    {
        public string BankName { get; set; } = string.Empty;
        public string? AccountRef { get; set; }
        public decimal Principal { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public DepositInterestType InterestType { get; set; } = DepositInterestType.CompoundQuarterly;
        public DateTime StartDate { get; set; }
        public string? Notes { get; set; }
        public DepositStatus Status { get; set; } = DepositStatus.Active;
    }
}

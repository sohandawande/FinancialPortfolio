using FinancialPortfolio.Data.Common.Base;
using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Data.Entities.Portfolio
{
    public sealed class PortfolioFixedDepositEntity : BaseEntity
    {
        public long Id { get; set; }
        public long PortfolioId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string? AccountRef { get; set; }
        public decimal Principal { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public DepositInterestType InterestType { get; set; } = DepositInterestType.CompoundQuarterly;
        public DateTime StartDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public DepositStatus Status { get; set; } = DepositStatus.Active;
        public string? Notes { get; set; }

        public PortfolioEntity Portfolio { get; set; } = default!;
    }
}

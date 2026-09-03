using FinancialPortfolio.Data.Common.Base;
using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Data.Entities.Portfolio
{
    public sealed class PortfolioInsurancePolicyEntity : BaseEntity
    {
        public long Id { get; set; }
        public long PortfolioId { get; set; }
        public string InsurerName { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public InsurancePolicyType PolicyType { get; set; } = InsurancePolicyType.Endowment;
        public decimal SumAssured { get; set; }
        public decimal PremiumAmount { get; set; }
        public PremiumFrequency PremiumFrequency { get; set; } = PremiumFrequency.Yearly;
        public int PremiumPayingTermYears { get; set; }
        public int PolicyTermYears { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime MaturityDate { get; set; }
        /// <summary>Number of premium installments already paid.</summary>
        public int PremiumsPaid { get; set; }
        /// <summary>Expected / declared maturity benefit (user entered). Optional for pure term.</summary>
        public decimal? ExpectedMaturityAmount { get; set; }
        public InsurancePolicyStatus Status { get; set; } = InsurancePolicyStatus.Active;
        public string? Notes { get; set; }

        public PortfolioEntity Portfolio { get; set; } = default!;
    }
}

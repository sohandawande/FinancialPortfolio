using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Response.Wealth
{
    public class InsurancePolicyResponse
    {
        public long Id { get; set; }
        public string InsurerName { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public InsurancePolicyType PolicyType { get; set; }
        public decimal SumAssured { get; set; }
        public decimal PremiumAmount { get; set; }
        public PremiumFrequency PremiumFrequency { get; set; }
        public int PremiumPayingTermYears { get; set; }
        public int PolicyTermYears { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public int PremiumsPaid { get; set; }
        public int MaxPremiumInstallments { get; set; }
        public decimal TotalPremiumsPaid { get; set; }
        public decimal? ExpectedMaturityAmount { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal GainLoss { get; set; }
        public InsurancePolicyStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}

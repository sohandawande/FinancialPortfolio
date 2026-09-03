using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Request.Wealth
{
    public class UpsertInsurancePolicyRequest
    {
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
        public int PremiumsPaid { get; set; }
        public decimal? ExpectedMaturityAmount { get; set; }
        public InsurancePolicyStatus Status { get; set; } = InsurancePolicyStatus.Active;
        public string? Notes { get; set; }
    }
}

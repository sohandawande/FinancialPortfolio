using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Business.Common.Helpers
{
    public static class InsuranceMath
    {
        public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

        /// <summary>How many premium installments occur in one year for the frequency.</summary>
        public static int InstallmentsPerYear(PremiumFrequency frequency) => frequency switch
        {
            PremiumFrequency.Weekly => 52,
            PremiumFrequency.Monthly => 12,
            PremiumFrequency.Quarterly => 4,
            PremiumFrequency.HalfYearly => 2,
            PremiumFrequency.Yearly => 1,
            PremiumFrequency.Single => 1,
            _ => 1
        };

        public static int MaxInstallments(PremiumFrequency frequency, int premiumPayingTermYears)
        {
            if (frequency == PremiumFrequency.Single) return 1;
            if (premiumPayingTermYears <= 0) return 0;
            return InstallmentsPerYear(frequency) * premiumPayingTermYears;
        }

        public static DateTime MaturityDate(DateTime start, int policyTermYears)
            => start.Date.AddYears(Math.Max(0, policyTermYears));

        public static decimal TotalPremiumsPaid(decimal premiumAmount, int premiumsPaid)
            => Round(Math.Max(0, premiumAmount) * Math.Max(0, premiumsPaid));

        /// <summary>
        /// Conservative current value for wealth summary:
        /// - Pure term: 0 (protection only)
        /// - Matured / surrendered with expected amount: use expected
        /// - Otherwise: total premiums paid (cost basis) until user supplies expected maturity.
        /// </summary>
        public static decimal CurrentValue(
            InsurancePolicyType type,
            decimal totalPremiumsPaid,
            decimal? expectedMaturity,
            InsurancePolicyStatus status,
            DateTime maturityDate,
            DateTime asOf)
        {
            if (type == InsurancePolicyType.Term)
                return 0;

            if (status is InsurancePolicyStatus.Matured or InsurancePolicyStatus.Surrendered or InsurancePolicyStatus.Closed
                || asOf.Date >= maturityDate.Date)
            {
                if (expectedMaturity.HasValue && expectedMaturity.Value > 0)
                    return Round(expectedMaturity.Value);
                return Round(totalPremiumsPaid);
            }

            // Active savings / ULIP style: cost basis unless expected is lower (rare)
            return Round(totalPremiumsPaid);
        }
    }
}

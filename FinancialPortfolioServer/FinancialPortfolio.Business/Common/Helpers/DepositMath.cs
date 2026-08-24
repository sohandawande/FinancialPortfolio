using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Business.Common.Helpers
{
    public static class DepositMath
    {
        public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

        public static DateTime MaturityDate(DateTime start, int months) => start.Date.AddMonths(months);

        public static decimal FdMaturity(decimal principal, decimal ratePct, int months, DepositInterestType type)
        {
            if (principal <= 0 || months <= 0) return Round(principal);
            var years = months / 12m;
            if (type == DepositInterestType.Simple)
                return Round(principal * (1 + ratePct / 100m * years));

            var quarters = months / 3m;
            var i = (double)(ratePct / 400m);
            return Round(principal * (decimal)Math.Pow(1 + i, (double)quarters));
        }

        public static decimal FdCurrent(decimal principal, decimal ratePct, int months, DateTime start, DateTime maturity, DateTime asOf, DepositInterestType type)
        {
            if (asOf.Date >= maturity.Date)
                return FdMaturity(principal, ratePct, months, type);

            var elapsedMonths = Math.Max(0, ((asOf.Year - start.Year) * 12) + asOf.Month - start.Month);
            elapsedMonths = Math.Min(elapsedMonths, months);
            return FdMaturity(principal, ratePct, elapsedMonths, type);
        }

        public static decimal RdMaturity(decimal installment, decimal ratePct, int tenureMonths)
        {
            if (installment <= 0 || tenureMonths <= 0) return 0;
            var i = (double)(ratePct / 400m);
            if (i <= 0)
                return Round(installment * tenureMonths);

            var factor = (Math.Pow(1 + i, tenureMonths) - 1) / i * (1 + i);
            return Round(installment * (decimal)factor);
        }

        public static int ElapsedInstallments(DateTime start, int tenureMonths, int recordedPaid, DateTime asOf)
        {
            var elapsed = Math.Max(0, ((asOf.Year - start.Year) * 12) + asOf.Month - start.Month + 1);
            var paid = Math.Max(recordedPaid, 0);
            return Math.Min(tenureMonths, Math.Max(paid, Math.Min(elapsed, tenureMonths)));
        }

        public static decimal RdCurrent(decimal installment, decimal ratePct, int tenureMonths, int paid, DateTime start, DateTime asOf)
        {
            var n = ElapsedInstallments(start, tenureMonths, paid, asOf);
            return RdMaturity(installment, ratePct, n);
        }
    }
}

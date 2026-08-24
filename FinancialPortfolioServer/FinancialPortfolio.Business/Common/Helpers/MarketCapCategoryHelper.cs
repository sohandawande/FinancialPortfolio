namespace FinancialPortfolio.Business.Common.Helpers
{
    /// <summary>
    /// Official NSE / SEBI-style equity buckets.
    /// Prefer index membership, then rank: 1–100 Large, 101–250 Mid, 251–500 Small, 501+ Micro.
    /// </summary>
    public static class MarketCapCategoryHelper
    {
        public const string LargeCap = "Large Cap";
        public const string MidCap = "Mid Cap";
        public const string SmallCap = "Small Cap";
        public const string MicroCap = "Micro Cap";
        public const string Sme = "SME";
        public const string Others = "Others";

        private static readonly HashSet<string> SmeSeries = new(StringComparer.OrdinalIgnoreCase)
        {
            "SM", "ST", "SG", "IT"
        };

        public static bool IsSmeSeries(string? series)
            => !string.IsNullOrWhiteSpace(series) && SmeSeries.Contains(series.Trim());

        public static string FromSeries(string? series)
            => IsSmeSeries(series) ? Sme : string.Empty;

        public static string FromRank(int rank)
        {
            if (rank <= 0)
                return string.Empty;

            return rank switch
            {
                <= 100 => LargeCap,
                <= 250 => MidCap,
                <= 500 => SmallCap,
                _ => MicroCap
            };
        }

        public static bool IsPlaceholder(string? category)
            => string.IsNullOrWhiteSpace(category)
               || category is "NSE" or "Equity" or "Others";
    }
}

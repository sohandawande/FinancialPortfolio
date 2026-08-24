namespace FinancialPortfolio.Business.Services.Wealth
{
    public sealed class MutualFundNavQuote
    {
        public int SchemeCode { get; set; }
        public string SchemeName { get; set; } = string.Empty;
        public string? Amc { get; set; }
        public decimal Nav { get; set; }
        public DateTime AsOf { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}

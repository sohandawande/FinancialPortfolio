namespace FinancialPortfolio.Models.Model.Response.Wealth
{
    public class MutualFundSchemeLookupResponse
    {
        public int SchemeCode { get; set; }
        public string SchemeName { get; set; } = string.Empty;
        public string? Amc { get; set; }
        public string Source { get; set; } = "mfapi";
    }
}

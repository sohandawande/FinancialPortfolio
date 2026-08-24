namespace FinancialPortfolio.Models.Model.Response.Wealth
{
    public class MutualFundNavSyncResponse
    {
        public int Updated { get; set; }
        public int Failed { get; set; }
        public int Skipped { get; set; }
        public string PrimarySource { get; set; } = "mfapi";
        public string? FallbackSource { get; set; } = "amfi";
        public List<string> Errors { get; set; } = [];
    }
}

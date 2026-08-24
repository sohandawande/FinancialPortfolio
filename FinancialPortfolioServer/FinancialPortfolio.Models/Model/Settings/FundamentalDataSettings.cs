namespace FinancialPortfolio.Models.Model.Settings
{
    /// <summary>
    /// Controls NSE-only stock fundamental-data enrichment.
    /// </summary>
    public sealed class FundamentalDataSettings
    {
        public const string SectionName = "FundamentalData";

        public bool Enabled { get; set; } = true;

        /// <summary>Number of previous NSE trading days to try when a report is unavailable.</summary>
        public int LookbackDays { get; set; } = 5;

        /// <summary>HTTP timeout for one NSE report request.</summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>Refresh existing values as well as missing values.</summary>
        public bool RefreshExistingValues { get; set; } = true;

        /// <summary>
        /// Maximum number of stocks for which NSE fundamentals are requested during one enrichment run.
        /// Set this in appsettings.json. For example: 500, 1000, or any other desired limit.
        /// </summary>
        public int MaxStocks { get; set; } = 500;
    }
}

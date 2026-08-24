namespace FinancialPortfolio.Models.Model.Settings
{
    public sealed class MutualFundNavSettings
    {
        public const string SectionName = "MutualFundNav";

        public bool Enabled { get; set; } = true;

        /// <summary>Primary JSON API. No key required.</summary>
        public string MfApiBaseUrl { get; set; } = "https://api.mfapi.in";

        /// <summary>Official AMFI daily NAV dump (fallback).</summary>
        public string AmfiNavUrl { get; set; } = "https://www.amfiindia.com/spages/NAVAll.txt";

        /// <summary>AMFI typically publishes after evening; default 21:00 IST.</summary>
        public int DailySyncHourIst { get; set; } = 21;

        public int DailySyncMinuteIst { get; set; } = 0;

        public int TimeoutSeconds { get; set; } = 30;
    }
}

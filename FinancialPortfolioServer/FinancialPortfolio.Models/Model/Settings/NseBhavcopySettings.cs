namespace FinancialPortfolio.Models.Model.Settings
{
    public sealed class NseBhavcopySettings
    {
        public const string SectionName = "NseBhavcopy";

        public bool Enabled { get; set; } = true;

        /// <summary>NSE cash series. EQ plus SME boards.</summary>
        public string[] NseSeries { get; set; } = ["EQ", "SM", "ST", "SG"];

        public int DailySyncHourIst { get; set; } = 18;

        public int DailySyncMinuteIst { get; set; } = 30;

        public int LookbackDays { get; set; } = 7;
    }
}

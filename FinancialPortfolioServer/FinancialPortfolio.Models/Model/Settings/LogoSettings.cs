namespace FinancialPortfolio.Models.Model.Settings
{
    public sealed class LogoSettings
    {
        public const string SectionName = "Logo";
        public string PublishableToken { get; set; } = string.Empty;
        public string ExchangeSuffix { get; set; } = ".NS";
        public string StorageFolder { get; set; } = "wwwroot/logos";
        public string PublicPathPrefix { get; set; } = "/logos";
    }
}

using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Business.Common.Logging
{
    public static class ApplicationLevelNames
    {
        public const string Api = "FinancialPortfolio.Api";
        public const string Business = "FinancialPortfolio.Business";
        public const string Data = "FinancialPortfolio.Data";
        public const string Models = "FinancialPortfolio.Models";
        public const string QueryEngine = "FinancialPortfolio.QueryEngine";

        /// <summary>Must match Angular client label.</summary>
        public const string FrontendClient = "FinancialPortfolio.Client";

        public static string ToStorage(ApplicationLevelType level) =>
            level switch
            {
                ApplicationLevelType.FrontendClient => FrontendClient,
                ApplicationLevelType.Api => Api,
                ApplicationLevelType.Business => Business,
                ApplicationLevelType.Data => Data,
                ApplicationLevelType.Models => Models,
                ApplicationLevelType.QueryEngine => QueryEngine,
                _ => level.ToString()
            };
    }
}

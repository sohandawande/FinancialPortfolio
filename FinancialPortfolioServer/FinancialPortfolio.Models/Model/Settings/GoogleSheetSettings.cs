namespace FinancialPortfolio.Models.Model.Settings
{
    public sealed class GoogleSheetSettings
    {
        public const string SectionName = "GoogleSheet";
        public string ApiKey { get; set; } = string.Empty;
        public string SpreadsheetId { get; set; } = string.Empty;
        public string SheetName { get; set; } = "Stocks";
    }
}

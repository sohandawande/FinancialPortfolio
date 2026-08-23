using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Settings;

namespace FinancialPortfolio.Business.Common.Urls
{
    public static class GoogleSheetUrlBuilder
    {
        private const string BaseUrl = "https://sheets.googleapis.com/v4/spreadsheets";

        public static string BuildReadUrl(GoogleSheetSettings settings)
        {
            settings = Guard.AgainstNull(settings, nameof(settings));
            return $"{BaseUrl}/{settings.SpreadsheetId}/values/{settings.SheetName}?key={settings.ApiKey}";
        }

        public static string BuildRangeUrl(GoogleSheetSettings settings, string range)
        {
            settings = Guard.AgainstNull(settings, nameof(settings));
            return $"{BaseUrl}/{settings.SpreadsheetId}/values/{range}?key={settings.ApiKey}";
        }
    }
}

using FinancialPortfolio.Business.Common.Constants;
using System.Globalization;

namespace FinancialPortfolio.Business.Common.Helpers
{
    public static class GoogleSheetHelper
    {
        private const decimal Lakh = 100000;
        private const decimal Crore = 10000000;
        private const decimal Billion = 1000000000;
        private const decimal Trillion = 1000000000000;
        public static string GetValue(IReadOnlyList<object> row, IReadOnlyDictionary<string, int> columns, string columnName)
        {
            return columns.TryGetValue(columnName, out var index) && index < row.Count
                ? row[index]?.ToString()?.Trim() ?? string.Empty
                : string.Empty;
        }

        public static decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || IsGoogleError(value))
                return 0m;

            value = value.Replace(",", "")
                         .Replace("₹", "")
                         .Replace("$", "")
                         .Trim();

            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : 0m;
        }

        public static long ParseLong(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || IsGoogleError(value))
                return 0L;

            value = value.Replace(",", "").Trim();

            return long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : 0L;
        }

        public static int ParseInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || IsGoogleError(value))
                return 0;

            value = value.Replace(",", "").Trim();

            return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : 0;
        }

        public static bool ParseBool(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || IsGoogleError(value))
                return false;

            value = value.Trim().ToLowerInvariant();

            return value switch
            {
                "true" or "yes" or "y" or "1" or "active" or "enabled" => true,
                "false" or "no" or "n" or "0" or "inactive" or "disabled" => false,
                _ => false
            };
        }

        public static DateTime? ParseDateTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || IsGoogleError(value))
                return null;

            value = value.Trim();

            // Try multiple common formats used in Google Sheets
            string[] formats =
            {
                "yyyy-MM-dd",
                "dd-MM-yyyy",
                "MM/dd/yyyy",
                "dd/MM/yyyy",
                "yyyy/MM/dd",
                "dd-MMM-yyyy",
                "MMM dd, yyyy",

                "yyyy-MM-dd HH:mm:ss",
                "dd-MM-yyyy HH:mm:ss",
                "dd/MM/yyyy HH:mm:ss",   // <-- ADD THIS
                "MM/dd/yyyy HH:mm:ss"
            };

            if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactResult))
                return exactResult;

            // Fallback to normal parse
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;

            return null;
        }

        public static DateOnly? ParseDateOnly(string? value)
        {
            var dateTime = ParseDateTime(value);
            return dateTime.HasValue ? DateOnly.FromDateTime(dateTime.Value) : null;
        }

        public static bool IsGoogleError(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   GoogleSheetConstants.ErrorValues.Contains(value.Trim());
        }

        public static string GetCategory(decimal marketCapInCrores)
        {
            return marketCapInCrores switch
            {
                > 20000 => "Large Cap",
                >= 5000 => "Mid Cap",
                >= 500 => "Small Cap",
                _ => "Micro Cap"
            };
        }

        public static decimal ToLakhs(decimal value, int decimals = 2)
        {
            return Math.Round(value / Lakh, decimals);
        }

        public static decimal ToCrores(decimal value, int decimals = 5)
        {
            return Math.Round(value / Crore, decimals);
        }

        public static decimal ToBillions(decimal value, int decimals = 2)
        {
            return Math.Round(value / Billion, decimals);
        }

        public static decimal ToTrillions(decimal value, int decimals = 2)
        {
            return Math.Round(value / Trillion, decimals);
        }
    }
}
using System.Runtime.CompilerServices;

namespace FinancialPortfolio.Business.Common.Logging
{
    public readonly record struct LogSource(string Category, string Method)
    {
        public static LogSource Of([CallerFilePath] string filePath = "", [CallerMemberName] string method = "")
        {
            return new(ToFileName(filePath), string.IsNullOrWhiteSpace(method) ? "Unknown" : method.Trim());
        }
        private static string ToFileName(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return "Unknown";
            }
            // "D:\...\AuthenticationService.cs" → "AuthenticationService.cs"
            return Path.GetFileName(filePath);
        }

        public override string ToString()
        {
            return $"{Category}.{Method}";
        }
    }
}

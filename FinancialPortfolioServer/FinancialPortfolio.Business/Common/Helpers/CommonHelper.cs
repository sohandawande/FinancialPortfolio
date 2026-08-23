namespace FinancialPortfolio.Business.Common.Helpers
{
    public static class CommonHelper
    {
        public static string BuildFullName(string? first, string? last) => $"{first} {last}".Trim();
    }
}

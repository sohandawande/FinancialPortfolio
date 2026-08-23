using Microsoft.AspNetCore.Http;

namespace FinancialPortfolio.Business.Common.Helpers
{
    public static class LogoUrlHelper
    {
        public static string? ToPublicUrl(string? logoUrl, HttpRequest? request)
        {
            if (string.IsNullOrWhiteSpace(logoUrl))
                return null;

            var value = logoUrl.Trim();
            if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return value;

            if (value.StartsWith("//"))
                return $"https:{value}";

            var path = value.StartsWith('/') ? value : "/" + value;
            if (request is null)
                return path;

            return $"{request.Scheme}://{request.Host.Value}{path}";
        }
    }
}

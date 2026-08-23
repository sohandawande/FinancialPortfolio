using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Request.SystemLog
{
    public class ClientLogRequest
    {
        public LogLevelType Level { get; set; } = LogLevelType.Error;           // Information, Warning, Error, Critical
        public string Category { get; set; } = "Angular";
        public string Method { get; set; } = string.Empty;
        public string? Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string? StackTrace { get; set; }
        public string? PageUrl { get; set; }
        public string? UserAgent { get; set; }
    }
}

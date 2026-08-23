using FinancialPortfolio.Models.Common.Base;

namespace FinancialPortfolio.Models.Model.Response.SystemLog
{
    public sealed class SystemLogResponse : BaseModel
    {
        public long Id { get; set; }
        public string LogLevel { get; set; } = string.Empty;
        public string ApplicationLevel { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? Message { get; set; } = string.Empty;
        public long? UserId { get; set; }
        public Guid? IdentityUserId { get; set; }
        public string? RequestPath { get; set; }
        public string? IpAddress { get; set; }
        public string? MachineName { get; set; }
        public SystemLogDetailResponse? Detail { get; set; }
    }
}

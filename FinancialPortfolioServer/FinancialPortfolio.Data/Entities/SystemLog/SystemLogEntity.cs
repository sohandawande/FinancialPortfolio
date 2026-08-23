using FinancialPortfolio.Data.Common.Base;
using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Data.Entities.SystemLog
{
    public class SystemLogEntity : BaseEntity
    {
        public long Id { get; set; }
        public LogLevelType LogLevel { get; set; }
        public string ApplicationLevel { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? Message { get; set; } = string.Empty;
        public long? UserId { get; set; }
        public Guid? IdentityUserId { get; set; }
        public string? RequestPath { get; set; }
        public string? IpAddress { get; set; }
        public string? MachineName { get; set; }
        public SystemLogDetailEntity? SystemLogDetail { get; set; }
    }
}

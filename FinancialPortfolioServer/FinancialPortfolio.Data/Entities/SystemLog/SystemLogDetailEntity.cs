using FinancialPortfolio.Data.Common.Base;

namespace FinancialPortfolio.Data.Entities.SystemLog
{
    public class SystemLogDetailEntity : BaseEntity
    {
        public long Id { get; set; }
        public long SystemLogId { get; set; }
        public string? Exception { get; set; }
        public string? StackTrace { get; set; }
        public SystemLogEntity SystemLog { get; set; } = default!;
    }
}

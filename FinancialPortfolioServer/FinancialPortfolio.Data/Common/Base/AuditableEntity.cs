namespace FinancialPortfolio.Data.Common.Base
{
    public abstract class AuditableEntity : BaseEntity
    {
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
    }
}

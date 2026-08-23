namespace FinancialPortfolio.Data.Common.Base
{
    public abstract class SoftDeleteEntity : BaseEntity
    {
        public bool IsDeleted { get; set; }
    }
}

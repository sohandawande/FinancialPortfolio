namespace FinancialPortfolio.Data.Common.Base
{
    public abstract class ActivatableEntity : BaseEntity
    {
        public bool IsActive { get; set; } = true;
    }
}

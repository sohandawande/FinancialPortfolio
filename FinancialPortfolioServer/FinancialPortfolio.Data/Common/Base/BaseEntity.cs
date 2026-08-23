namespace FinancialPortfolio.Data.Common.Base
{
    /// <summary>
    /// Defines the <see cref="BaseEntity" />
    /// </summary>
    public abstract class BaseEntity
    {
        public long? CreatedBy { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}

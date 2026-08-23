namespace FinancialPortfolio.Models.Common.Base
{
    /// <summary>
    /// Defines the <see cref="BaseModel" />
    /// </summary>
    public abstract class BaseModel
    {
        /// <summary>
        /// Gets or sets the CreatedBy
        /// </summary>
        public long? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the ModifiedBy
        /// </summary>
        public long? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the CreatedDate
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the ModifiedDate
        /// </summary>
        public DateTime ModifiedDate { get; set; }
    }
}

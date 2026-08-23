namespace FinancialPortfolio.Data.Entities.Stock
{
    using FinancialPortfolio.Data.Common.Base;

    /// <summary>
    /// Defines the <see cref="StockEntity" />
    /// </summary>
    public sealed class StockEntity : BaseEntity
    {
        public long Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string ISINCode { get; set; } = string.Empty;
        public string Series { get; set; } = string.Empty;
        public StockDetailEntity? StockDetail { get; set; }
    }
}

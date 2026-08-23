using FinancialPortfolio.Data.Common.Base;
using FinancialPortfolio.Data.Entities.Stock;

namespace FinancialPortfolio.Data.Entities.Portfolio
{
    public sealed class PortfolioStockDividendEntity : BaseEntity
    {
        public long Id { get; set; }
        public long PortfolioId { get; set; }
        public long StockId { get; set; }
        public int Quantity { get; set; }
        public decimal PerShareAmount { get; set; }
        public decimal Amount { get; set; }
        public DateTime DividendDate { get; set; }
        public DateTime? ExDate { get; set; }
        public DateTime? RecordDate { get; set; }
        public string? Notes { get; set; }

        public PortfolioEntity Portfolio { get; set; } = default!;
        public StockEntity Stock { get; set; } = default!;
    }
}

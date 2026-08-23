using FinancialPortfolio.Data.Common.Base;
using FinancialPortfolio.Data.Entities.Stock;
using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Data.Entities.Portfolio
{
    public sealed class PortfolioStockHoldEntity : BaseEntity
    {
        public long Id { get; set; }
        public long PortfolioId { get; set; }
        public long StockId { get; set; }
        public StockExchange Exchange { get; set; } = StockExchange.NSE;
        public int Quantity { get; set; }
        public int RemainingQuantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal InvestmentAmount { get; set; }
        public long? HoldDays { get; set; }
        public LotStatus LotStatus { get; set; } = LotStatus.Open;
        public bool IsSold { get; set; } = false;
        public DateTime PurchaseDate { get; set; }
        public DateTime? ExitDate { get; set; }
        public string? HoldNotes { get; set; }

        // Navigation
        public PortfolioEntity Portfolio { get; set; } = default!;
        public StockEntity Stock { get; set; } = default!;
        public ICollection<PortfolioStockSoldEntity> PortfolioStockSolds { get; set; } = [];
    }
}

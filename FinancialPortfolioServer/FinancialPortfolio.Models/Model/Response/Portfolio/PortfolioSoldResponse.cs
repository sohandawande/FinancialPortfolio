using FinancialPortfolio.Models.Common.Base;
using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class PortfolioSoldResponse : BaseModel
    {
        public long Id { get; set; }                           // Sold Id
        public long HoldId { get; set; }
        public long StockId { get; set; }

        // Stock info
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public StockExchange Exchange { get; set; }

        // Sell details
        public int SellQuantity { get; set; }
        public decimal SellPrice { get; set; }
        public decimal SellAmount { get; set; }                 // SellQty × SellPrice

        // Original buy context
        public decimal PurchasePrice { get; set; }
        public decimal CostAmount { get; set; }                 // SellQty × PurchasePrice

        // Realized P&L
        public decimal RealizedGainLoss { get; set; }
        public decimal RealizedGainLossPercent { get; set; }

        // Status & Dates
        public long? HoldDays { get; set; }
        public LotStatus LotStatus { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime SoldDate { get; set; }
        public string? SoldNotes { get; set; }
    }
}

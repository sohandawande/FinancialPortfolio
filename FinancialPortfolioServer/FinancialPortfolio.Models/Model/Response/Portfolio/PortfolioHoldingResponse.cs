using FinancialPortfolio.Models.Common.Base;
using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class PortfolioHoldingResponse : BaseModel
    {
        public long Id { get; set; }                           // Hold Id
        public long PortfolioId { get; set; }
        public long StockId { get; set; }

        // Stock info
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public StockExchange Exchange { get; set; }

        // Quantities & Prices
        public int Quantity { get; set; }                      // Original buy qty
        public int RemainingQuantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal InvestmentAmount { get; set; }          // Original investment
        public decimal RemainingInvestment { get; set; }       // RemainingQty × PurchasePrice

        // Live market
        public decimal CurrentPrice { get; set; }
        public decimal CurrentValue { get; set; }               // RemainingQty × CurrentPrice

        // P&L
        public decimal UnrealizedGainLoss { get; set; }
        public decimal UnrealizedGainLossPercent { get; set; }

        // Status & Dates
        public long? HoldDays { get; set; }
        public LotStatus LotStatus { get; set; }
        public bool IsSold { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? ExitDate { get; set; }
        public string? HoldNotes { get; set; }
    }
}

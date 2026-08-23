using FinancialPortfolio.Models.Common.Base;
using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class PortfolioLedgerItemResponse : BaseModel
    {
        public int SerialNo { get; set; }
        public long Id { get; set; }
        public long? HoldId { get; set; }
        public long? SoldId { get; set; }
        public long StockId { get; set; }

        public string CompanyName { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string StockCode { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public StockExchange Exchange { get; set; }

        public int NetQuantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal MarketPrice { get; set; }
        public decimal TotalInvestment { get; set; }
        public decimal TotalCurrentValue { get; set; }
        public decimal TotalGainLoss { get; set; }
        public decimal GainLossPercent { get; set; }
        public long HoldDays { get; set; }

        public InvestmentAction CurrentType { get; set; }
        public DateTime AsOfDate { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? ExitDate { get; set; }
        public decimal? SellPrice { get; set; }
        public decimal? TotalOnSell { get; set; }
        public ProfitLoss ProfitLoss { get; set; }
    }
}

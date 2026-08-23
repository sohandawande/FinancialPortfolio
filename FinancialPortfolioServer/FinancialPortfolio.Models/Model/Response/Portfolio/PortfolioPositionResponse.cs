using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class PortfolioPositionResponse
    {
        public int SerialNo { get; set; }
        public long StockId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string StockCode { get; set; } = string.Empty;
        public StockExchange Exchange { get; set; }

        public int CurrentQuantity { get; set; }
        public int LifetimeBoughtQuantity { get; set; }
        public int LifetimeSoldQuantity { get; set; }

        public decimal AverageBuyPrice { get; set; }
        public decimal LifetimeAverageBuyPrice { get; set; }
        public decimal? AverageSellPrice { get; set; }
        public decimal MarketPrice { get; set; }

        public decimal TotalInvestment { get; set; }
        public decimal LifetimeInvestment { get; set; }
        public decimal TotalCurrentValue { get; set; }
        public decimal TotalOnSell { get; set; }

        public decimal UnrealizedGainLoss { get; set; }
        public decimal UnrealizedGainLossPercent { get; set; }
        public decimal RealizedGainLoss { get; set; }
        public decimal RealizedGainLossPercent { get; set; }
        public decimal TotalDividends { get; set; }
        public decimal TotalGainLoss { get; set; }
        public decimal GainLossPercent { get; set; }
        public ProfitLoss ProfitLoss { get; set; }

        public long HoldDays { get; set; }
        public PositionStatus Status { get; set; }
        public InvestmentAction CurrentType { get; set; }

        public int BuyLotCount { get; set; }
        public int OpenLotCount { get; set; }
        public int SellCount { get; set; }
        public int DividendCount { get; set; }

        public DateTime FirstPurchaseDate { get; set; }
        public DateTime? LastExitDate { get; set; }
        public DateTime LastActivityDate { get; set; }
        public DateTime AsOfDate { get; set; }
    }
}

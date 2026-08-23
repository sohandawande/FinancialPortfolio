namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class FifoSellResponse
    {
        public long StockId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public int TotalSellQuantity { get; set; }
        public decimal SellPrice { get; set; }
        public decimal TotalSellAmount { get; set; }
        public decimal TotalCostAmount { get; set; }
        public decimal TotalRealizedGainLoss { get; set; }
        public decimal TotalRealizedGainLossPercent { get; set; }
        public int LotsConsumed { get; set; }
        public List<PortfolioSoldResponse> Allocations { get; set; } = [];
    }
}

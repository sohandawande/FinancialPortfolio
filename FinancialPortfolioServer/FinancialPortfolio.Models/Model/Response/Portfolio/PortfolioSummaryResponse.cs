namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class PortfolioSummaryResponse
    {
        public long PortfolioId { get; set; }
        public string Name { get; set; } = "My Portfolio";

        // Investment & Value
        public decimal TotalInvestment { get; set; }           // Remaining holdings cost
        public decimal TotalCurrentValue { get; set; }         // RemainingQty × CurrentPrice
        public decimal UnrealizedGainLoss { get; set; }
        public decimal UnrealizedGainLossPercent { get; set; }

        // Realized
        public decimal RealizedProfitBooked { get; set; }      // From all sells
        public decimal TotalDividendsReceived { get; set; }
        public decimal TotalBrokerageTax { get; set; }         // Placeholder for future

        // Counts
        public int TotalHoldLots { get; set; }                 // Open + Partial lots
        public int TotalSoldLots { get; set; }
        public int TotalStocksHold { get; set; }               // Distinct stocks still held
        public int TotalStocksSold { get; set; }
        public int TotalStocksHoldSell { get; set; }           // Hold + Sold distinct

        public DateTime? LastUpdated { get; set; }
        public List<PortfolioDividendYearTotalResponse> DividendsByYear { get; set; } = [];
    }
}

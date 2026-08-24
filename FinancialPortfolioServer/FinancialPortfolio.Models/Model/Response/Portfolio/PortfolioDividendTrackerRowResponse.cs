namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class PortfolioDividendTrackerRowResponse
    {
        public long StockId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public int CurrentQuantity { get; set; }
        public decimal Invested { get; set; }
        public decimal ThisYearAmount { get; set; }
        public decimal LifetimeAmount { get; set; }
        public decimal YieldOnCostPercent { get; set; }
        public int PayoutCount { get; set; }
        public DateTime? LastDividendDate { get; set; }
    }
}

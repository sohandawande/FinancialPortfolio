namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class PortfolioPositionEventResponse
    {
        public string EventType { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
        public long? SourceId { get; set; }
    }
}

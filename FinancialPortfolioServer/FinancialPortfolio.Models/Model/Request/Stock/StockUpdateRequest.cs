namespace FinancialPortfolio.Models.Model.Request.Stock
{
    public class StockUpdateRequest
    {
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string ISINCode { get; set; } = string.Empty;
        public string Series { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public decimal MarketCap { get; set; }
    }
}

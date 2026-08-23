using FinancialPortfolio.Models.Common.Base;

namespace FinancialPortfolio.Models.Model.Response.Stock
{
    public class StocksResponse : BaseModel
    {
        public long Id { get; set; }
        public long StockId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string ISINCode { get; set; } = string.Empty;
        public string Series { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public decimal PreviousClose { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public long Volume { get; set; }
        public long AverageVolume { get; set; }
        public decimal Week52High { get; set; }
        public decimal Week52Low { get; set; }
        public decimal PE { get; set; }
        public decimal EPS { get; set; }
        public decimal MarketCap { get; set; }
        public decimal PriceChange { get; set; }
        public bool IsActive { get; set; }
        public string? LogoUrl { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}

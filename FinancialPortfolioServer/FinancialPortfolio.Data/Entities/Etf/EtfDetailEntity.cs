using FinancialPortfolio.Data.Common.Base;

namespace FinancialPortfolio.Data.Entities.Etf
{
    public sealed class EtfDetailEntity : BaseEntity
    {
        public long Id { get; set; }
        public long EtfId { get; set; }
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
        public decimal PriceChangePercentage { get; set; }
        public bool IsActive { get; set; }
        public string? LogoUrl { get; set; }
        public DateTime? LastUpdated { get; set; }
        public EtfEntity Etf { get; set; } = default!;
    }
}

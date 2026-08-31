using FinancialPortfolio.Data.Common.Base;

namespace FinancialPortfolio.Data.Entities.Etf
{
    public sealed class EtfEntity : BaseEntity
    {
        public long Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string ISINCode { get; set; } = string.Empty;
        public string Series { get; set; } = string.Empty;
        public EtfDetailEntity? EtfDetail { get; set; }
    }
}

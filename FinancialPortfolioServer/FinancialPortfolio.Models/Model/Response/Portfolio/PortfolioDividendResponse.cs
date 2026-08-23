using FinancialPortfolio.Models.Common.Base;
using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class PortfolioDividendResponse : BaseModel
    {
        public long Id { get; set; }
        public long PortfolioId { get; set; }
        public long StockId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public StockExchange Exchange { get; set; }
        public int Quantity { get; set; }
        public decimal PerShareAmount { get; set; }
        public decimal Amount { get; set; }
        public DateTime DividendDate { get; set; }
        public DateTime? ExDate { get; set; }
        public DateTime? RecordDate { get; set; }
        public string? Notes { get; set; }
    }
}

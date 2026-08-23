using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class PortfolioDividendStockGroupResponse
    {
        public long StockId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public StockExchange Exchange { get; set; }
        public decimal TotalAmount { get; set; }
        public int PayoutCount { get; set; }
        public int TotalShares { get; set; }
        public DateTime? LastDividendDate { get; set; }
        public List<PortfolioDividendResponse> Payouts { get; set; } = [];
    }
}

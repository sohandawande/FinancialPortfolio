using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Request.Portfolio
{
    public class UpdateHoldRequest
    {
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public StockExchange Exchange { get; set; } = StockExchange.NSE;
        public string? Notes { get; set; }
    }
}

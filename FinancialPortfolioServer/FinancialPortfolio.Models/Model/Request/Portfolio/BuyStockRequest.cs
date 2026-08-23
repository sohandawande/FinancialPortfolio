using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Request.Portfolio
{
    public class BuyStockRequest
    {
        public long StockId { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public StockExchange Exchange { get; set; } = StockExchange.NSE;
        public string? Notes { get; set; }
    }
}

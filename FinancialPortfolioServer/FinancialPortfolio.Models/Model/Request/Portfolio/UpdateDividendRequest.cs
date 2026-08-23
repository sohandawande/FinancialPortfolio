namespace FinancialPortfolio.Models.Model.Request.Portfolio
{
    public class UpdateDividendRequest
    {
        public long StockId { get; set; }
        public int Quantity { get; set; }
        public decimal PerShareAmount { get; set; }
        public decimal? Amount { get; set; }
        public DateTime DividendDate { get; set; }
        public DateTime? ExDate { get; set; }
        public DateTime? RecordDate { get; set; }
        public string? Notes { get; set; }
    }
}

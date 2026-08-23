namespace FinancialPortfolio.Models.Model.Request.Portfolio
{
    public class UpdateSoldRequest
    {
        public int SellQuantity { get; set; }
        public decimal SellPrice { get; set; }
        public DateTime SoldDate { get; set; }
        public string? Notes { get; set; }
    }
}

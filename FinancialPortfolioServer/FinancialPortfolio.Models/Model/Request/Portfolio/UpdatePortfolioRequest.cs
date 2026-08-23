namespace FinancialPortfolio.Models.Model.Request.Portfolio
{
    public class UpdatePortfolioRequest
    {
        public string Name { get; set; } = "My Portfolio";
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

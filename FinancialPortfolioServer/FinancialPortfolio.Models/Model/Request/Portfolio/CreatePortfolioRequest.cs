namespace FinancialPortfolio.Models.Model.Request.Portfolio
{
    public class CreatePortfolioRequest
    {
        public string Name { get; set; } = "My Portfolio";
        public string? Description { get; set; }
    }
}

using FinancialPortfolio.Models.Common.Base;

namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class PortfolioResponse : BaseModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; } = "My Portfolio";
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}

using FinancialPortfolio.Data.Common.Base;
using FinancialPortfolio.Data.Entities.AppUser;

namespace FinancialPortfolio.Data.Entities.Portfolio
{
    public sealed class PortfolioEntity : BaseEntity
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; } = "My Portfolio";
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public AppUserEntity? User { get; set; }
        public ICollection<PortfolioStockHoldEntity> PortfolioStockHolds { get; set; } = [];
        public ICollection<PortfolioStockDividendEntity> PortfolioStockDividends { get; set; } = [];
    }
}

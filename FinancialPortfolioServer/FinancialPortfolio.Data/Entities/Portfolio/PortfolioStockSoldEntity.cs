using FinancialPortfolio.Data.Common.Base;
using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Data.Entities.Portfolio
{
    public sealed class PortfolioStockSoldEntity : BaseEntity
    {
        public long Id { get; set; }
        public long PortfolioStockHoldId { get; set; }
        public int SellQuantity { get; set; }
        public decimal SellPrice { get; set; }
        public long? HoldDays { get; set; }
        public LotStatus LotStatus { get; set; } = LotStatus.FullySold;
        public DateTime SoldDate { get; set; }
        public string? SoldNotes { get; set; }

        // Navigation
        public PortfolioStockHoldEntity PortfolioStockHold { get; set; } = default!;
    }
}

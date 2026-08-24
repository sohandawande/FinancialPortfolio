using FinancialPortfolio.Data.Common.Base;
using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Data.Entities.Portfolio
{
    public sealed class PortfolioMutualFundEntity : BaseEntity
    {
        public long Id { get; set; }
        public long PortfolioId { get; set; }
        public string SchemeName { get; set; } = string.Empty;
        public string Amc { get; set; } = string.Empty;
        public string? FolioNumber { get; set; }
        public int? SchemeCode { get; set; }
        public DateTime? NavAsOf { get; set; }
        public string? NavSource { get; set; }
        public MutualFundSchemeType SchemeType { get; set; } = MutualFundSchemeType.Equity;
        public decimal Units { get; set; }
        public decimal AverageNav { get; set; }
        public decimal CurrentNav { get; set; }
        public decimal InvestedAmount { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;

        public PortfolioEntity Portfolio { get; set; } = default!;
    }
}

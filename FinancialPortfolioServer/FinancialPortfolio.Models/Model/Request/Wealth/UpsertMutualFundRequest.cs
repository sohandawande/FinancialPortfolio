using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Request.Wealth
{
    public class UpsertMutualFundRequest
    {
        public string SchemeName { get; set; } = string.Empty;
        public string Amc { get; set; } = string.Empty;
        public string? FolioNumber { get; set; }
        public int? SchemeCode { get; set; }
        public MutualFundSchemeType SchemeType { get; set; } = MutualFundSchemeType.Equity;
        public decimal Units { get; set; }
        public decimal AverageNav { get; set; }
        public decimal CurrentNav { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

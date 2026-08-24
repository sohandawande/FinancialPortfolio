using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Models.Model.Response.Wealth
{
    public class MutualFundResponse
    {
        public long Id { get; set; }
        public string SchemeName { get; set; } = string.Empty;
        public string Amc { get; set; } = string.Empty;
        public string? FolioNumber { get; set; }
        public int? SchemeCode { get; set; }
        public DateTime? NavAsOf { get; set; }
        public string? NavSource { get; set; }
        public MutualFundSchemeType SchemeType { get; set; }
        public decimal Units { get; set; }
        public decimal AverageNav { get; set; }
        public decimal CurrentNav { get; set; }
        public decimal InvestedAmount { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal GainLoss { get; set; }
        public decimal GainLossPercent { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
    }
}

namespace FinancialPortfolio.Business.Services.MarketData
{
    public sealed class NseMcapRow
    {
        public string Symbol { get; init; } = string.Empty;
        public string Series { get; init; } = "EQ";

        /// <summary>Market capitalisation in ₹ Crore.</summary>
        public decimal MarketCapCrore { get; init; }
    }
}

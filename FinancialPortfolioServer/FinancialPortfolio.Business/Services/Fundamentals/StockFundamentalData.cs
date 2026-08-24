namespace FinancialPortfolio.Business.Services.Fundamentals
{
    /// <summary>
    /// Provider-neutral normalized fundamental values for one stock.
    /// Monetary market cap is expressed in INR crore.
    /// </summary>
    public sealed class StockFundamentalData
    {
        public string Symbol { get; init; } = string.Empty;
        public decimal? MarketCapCrore { get; init; }
        public decimal? PE { get; init; }
        public decimal? EPS { get; init; }
        public DateTimeOffset RetrievedAt { get; init; }
        public string Source { get; init; } = string.Empty;
    }
}

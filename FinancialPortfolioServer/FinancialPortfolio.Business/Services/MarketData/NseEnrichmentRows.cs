namespace FinancialPortfolio.Business.Services.MarketData
{
    public sealed class NsePeRow
    {
        public string Symbol { get; init; } = string.Empty;
        public string Series { get; init; } = "EQ";
        public decimal PE { get; init; }
        public decimal EPS { get; init; }
    }

    public sealed class NseWeek52Row
    {
        public string Symbol { get; init; } = string.Empty;
        public string Series { get; init; } = "EQ";
        public decimal High { get; init; }
        public decimal Low { get; init; }
    }

    public sealed class NseIndexMemberRow
    {
        public string Symbol { get; init; } = string.Empty;
        public string Industry { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
    }
}

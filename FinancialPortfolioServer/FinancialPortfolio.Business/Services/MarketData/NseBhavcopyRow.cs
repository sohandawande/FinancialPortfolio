namespace FinancialPortfolio.Business.Services.MarketData
{
    public sealed class NseBhavcopyRow
    {
        public string Exchange { get; init; } = "NSE";
        public DateOnly TradeDate { get; init; }
        public string Symbol { get; init; } = string.Empty;
        public string CompanyName { get; init; } = string.Empty;
        public string Isin { get; init; } = string.Empty;
        public string Series { get; init; } = string.Empty;
        public decimal Open { get; init; }
        public decimal High { get; init; }
        public decimal Low { get; init; }
        public decimal Close { get; init; }
        public decimal PreviousClose { get; init; }
        public long Volume { get; init; }
    }
}

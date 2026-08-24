namespace FinancialPortfolio.Models.Model.Request.MarketData
{
    public sealed class MarketDataSyncRequest
    {
        /// <summary>When true, walk back lookback days even if a recent file already exists.</summary>
        public bool ForceRefresh { get; set; }

        /// <summary>Optional trading date (yyyy-MM-dd). Null = latest available session.</summary>
        public DateOnly? TradeDate { get; set; }
    }
}

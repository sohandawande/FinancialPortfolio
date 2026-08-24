namespace FinancialPortfolio.Models.Model.Response.MarketData
{
    public sealed class MarketDataSyncResponse
    {
        public string Source { get; set; } = "NSE";
        public DateOnly? TradeDate { get; set; }
        public int TotalRecords { get; set; }
        public int InsertedRecords { get; set; }
        public int UpdatedRecords { get; set; }
        public int SkippedRecords { get; set; }
        public int NseRecords { get; set; }
        public int FundamentalRecords { get; set; }
        public int McapUpdated { get; set; }
        public int CapClassified { get; set; }
        public int PeUpdated { get; set; }
        public int EpsUpdated { get; set; }
        public int Week52Updated { get; set; }
        public int IndustryUpdated { get; set; }
    }
}

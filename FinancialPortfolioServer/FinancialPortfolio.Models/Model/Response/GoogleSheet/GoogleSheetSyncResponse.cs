namespace FinancialPortfolio.Models.Model.Response.GoogleSheet
{
    public sealed class GoogleSheetSyncResponse
    {
        public int TotalRecords { get; set; }
        public int InsertedRecords { get; set; }
        public int UpdatedRecords { get; set; }
        public int SkippedRecords { get; set; }
    }
}

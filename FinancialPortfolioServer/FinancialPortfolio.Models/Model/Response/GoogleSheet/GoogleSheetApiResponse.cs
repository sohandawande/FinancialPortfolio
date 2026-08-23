namespace FinancialPortfolio.Models.Model.Response.GoogleSheet
{
    public sealed class GoogleSheetApiResponse
    {
        public string Range { get; set; } = string.Empty;
        public string MajorDimension { get; set; } = string.Empty;
        public List<List<object>> Values { get; set; } = [];
    }
}

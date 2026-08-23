namespace FinancialPortfolio.Models.Common.Response
{
    public sealed class ExceptionResponse
    {
        public int StatusCode { get; set; }

        public string Message { get; set; } = string.Empty;

        public List<string> Errors { get; set; } = [];
    }
}

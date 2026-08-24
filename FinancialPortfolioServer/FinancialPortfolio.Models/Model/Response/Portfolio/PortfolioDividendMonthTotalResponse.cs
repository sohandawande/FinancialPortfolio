namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class PortfolioDividendMonthTotalResponse
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
        public int PayoutCount { get; set; }
    }
}

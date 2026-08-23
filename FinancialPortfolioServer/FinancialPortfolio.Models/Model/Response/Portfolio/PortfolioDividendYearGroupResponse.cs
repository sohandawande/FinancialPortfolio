namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class PortfolioDividendYearGroupResponse
    {
        public int Year { get; set; }
        public decimal Amount { get; set; }
        public int PayoutCount { get; set; }
        public int CompanyCount { get; set; }
        public List<PortfolioDividendResponse> Payouts { get; set; } = [];
    }
}

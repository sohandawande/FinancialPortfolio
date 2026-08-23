namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class PortfolioDividendOverviewResponse
    {
        public decimal TotalAmount { get; set; }
        public int CompanyCount { get; set; }
        public int PayoutCount { get; set; }
        public List<PortfolioDividendStockGroupResponse> Stocks { get; set; } = [];
        public List<PortfolioDividendYearGroupResponse> Years { get; set; } = [];
        public List<PortfolioDividendResponse> Payouts { get; set; } = [];
    }
}

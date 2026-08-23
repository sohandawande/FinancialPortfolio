namespace FinancialPortfolio.Models.Model.Response.Portfolio
{
    public class PortfolioPositionDetailResponse
    {
        public PortfolioPositionResponse Position { get; set; } = new();
        public List<PortfolioHoldingResponse> Buys { get; set; } = [];
        public List<PortfolioSoldResponse> Sells { get; set; } = [];
        public List<PortfolioDividendResponse> Dividends { get; set; } = [];
        public List<PortfolioPositionEventResponse> Timeline { get; set; } = [];
        public List<PortfolioDividendYearTotalResponse> DividendsByYear { get; set; } = [];
    }
}

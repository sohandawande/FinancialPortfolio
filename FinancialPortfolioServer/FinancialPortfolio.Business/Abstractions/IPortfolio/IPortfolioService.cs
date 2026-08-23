using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Model.Request.Portfolio;
using FinancialPortfolio.Models.Model.Response.Portfolio;

namespace FinancialPortfolio.Business.Abstractions.IPortfolio
{
    public interface IPortfolioService
    {
        Task<ApiResponse<PortfolioResponse?>> GetAsync(CancellationToken cancellationToken);
        Task<ApiResponse<PortfolioResponse>> CreateAsync(CreatePortfolioRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<PortfolioResponse>> UpdateAsync(UpdatePortfolioRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<PortfolioSummaryResponse>> GetSummaryAsync(CancellationToken cancellationToken);
        Task<ApiResponse<List<PortfolioHoldingResponse>>> GetHoldingsAsync(CancellationToken cancellationToken);
        Task<ApiResponse<List<PortfolioSoldResponse>>> GetSoldHistoryAsync(CancellationToken cancellationToken);
        Task<ApiResponse<PortfolioHoldingResponse>> BuyAsync(BuyStockRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<PortfolioHoldingResponse>> UpdateHoldAsync(long holdId, UpdateHoldRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<bool>> DeleteHoldAsync(long holdId, CancellationToken cancellationToken);
        Task<ApiResponse<FifoSellResponse>> SellAsync(SellStockRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<PortfolioSoldResponse>> UpdateSoldAsync(long soldId, UpdateSoldRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<bool>> DeleteSoldAsync(long soldId, CancellationToken cancellationToken);
        Task<ApiResponse<List<PortfolioLedgerItemResponse>>> GetLedgerAsync(string? type, CancellationToken cancellationToken);
        Task<ApiResponse<List<PortfolioPositionResponse>>> GetPositionsAsync(string? status, CancellationToken cancellationToken);
        Task<ApiResponse<PortfolioPositionDetailResponse>> GetPositionDetailAsync(long stockId, CancellationToken cancellationToken);
        Task<ApiResponse<List<PortfolioDividendResponse>>> GetDividendsAsync(long? stockId, CancellationToken cancellationToken);
        Task<ApiResponse<PortfolioDividendOverviewResponse>> GetDividendOverviewAsync(CancellationToken cancellationToken);
        Task<ApiResponse<PortfolioDividendResponse>> AddDividendAsync(AddDividendRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<PortfolioDividendResponse>> UpdateDividendAsync(long dividendId, UpdateDividendRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<bool>> DeleteDividendAsync(long dividendId, CancellationToken cancellationToken);
    }
}

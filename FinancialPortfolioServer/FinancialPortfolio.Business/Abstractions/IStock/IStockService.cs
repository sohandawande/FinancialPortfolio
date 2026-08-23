using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Model.Request.Stock;
using FinancialPortfolio.Models.Model.Response.Stock;
using FinancialPortfolio.QueryEngine.Models;

namespace FinancialPortfolio.Business.Abstractions.IStock
{
    public interface IStockService
    {
        Task<ApiResponse<PagedResponse<StocksResponse>>> SearchAsync(QueryRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<StocksResponse>> GetByIdAsync(long id, CancellationToken cancellationToken);
        Task<ApiResponse<StocksResponse>> CreateAsync(StockCreateRequest stockCreateRequest, CancellationToken cancellationToken);
        Task<ApiResponse<StocksResponse>> UpdateAsync(long id, StockUpdateRequest stockUpdateRequest, CancellationToken cancellationToken);
        Task<ApiResponse<StocksResponse>> DeleteAsync(long id, CancellationToken cancellationToken);
    }
}

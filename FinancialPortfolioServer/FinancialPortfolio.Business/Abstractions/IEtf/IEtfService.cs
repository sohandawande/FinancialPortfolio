using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Model.Request.Etf;
using FinancialPortfolio.Models.Model.Response.Etf;
using FinancialPortfolio.QueryEngine.Models;

namespace FinancialPortfolio.Business.Abstractions.IEtf
{
    public interface IEtfService
    {
        Task<ApiResponse<PagedResponse<EtfsResponse>>> SearchAsync(QueryRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<EtfsResponse>> GetByIdAsync(long id, CancellationToken cancellationToken);
        Task<ApiResponse<EtfsResponse>> CreateAsync(EtfCreateRequest stockCreateRequest, CancellationToken cancellationToken);
        Task<ApiResponse<EtfsResponse>> UpdateAsync(long id, EtfUpdateRequest stockUpdateRequest, CancellationToken cancellationToken);
        Task<ApiResponse<EtfsResponse>> DeleteAsync(long id, CancellationToken cancellationToken);
    }
}

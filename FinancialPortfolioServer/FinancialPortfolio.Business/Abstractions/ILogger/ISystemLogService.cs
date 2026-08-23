using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Model.Request.SystemLog;
using FinancialPortfolio.Models.Model.Response.SystemLog;
using FinancialPortfolio.QueryEngine.Models;

namespace FinancialPortfolio.Business.Abstractions.ILogger
{
    public interface ISystemLogService
    {
        Task<ApiResponse<PagedResponse<SystemLogResponse>>> GetAllAsync(QueryRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<SystemLogResponse>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> CreateClientLogAsync(ClientLogRequest request, CancellationToken cancellationToken = default);
    }
}

using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Model.Response.Wealth;

namespace FinancialPortfolio.Business.Abstractions.IWealth
{
    public interface IMutualFundNavService
    {
        Task<ApiResponse<List<MutualFundSchemeLookupResponse>>> SearchAsync(string query, CancellationToken cancellationToken);
        Task<ApiResponse<MutualFundNavSyncResponse>> SyncPortfolioNavAsync(CancellationToken cancellationToken);
        Task<ApiResponse<MutualFundNavSyncResponse>> SyncOneAsync(long mutualFundId, CancellationToken cancellationToken);
        Task<MutualFundNavSyncResponse> SyncAllActiveAsync(CancellationToken cancellationToken);
    }
}

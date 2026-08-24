using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Model.Request.MarketData;
using FinancialPortfolio.Models.Model.Response.MarketData;

namespace FinancialPortfolio.Business.Abstractions.IMarketData
{
    public interface INseBhavcopyService
    {
        Task<ApiResponse<MarketDataSyncResponse>> SyncAsync(MarketDataSyncRequest request, CancellationToken cancellationToken = default);
    }
}

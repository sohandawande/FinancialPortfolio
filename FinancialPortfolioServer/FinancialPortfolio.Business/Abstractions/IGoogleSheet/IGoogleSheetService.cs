using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Model.Request.GoogleSheet;
using FinancialPortfolio.Models.Model.Response.GoogleSheet;

namespace FinancialPortfolio.Business.Abstractions.IGoogleSheet
{
    public interface IGoogleSheetService
    {
        Task<ApiResponse<GoogleSheetSyncResponse>> SyncStocksAsync(GoogleSheetRequest request, CancellationToken cancellationToken = default);
    }
}

using FinancialPortfolio.Models.Model.Response.GoogleSheet;

namespace FinancialPortfolio.Business.Abstractions.IGoogleSheet
{
    public interface IGoogleSheetSyncService
    {
        Task<GoogleSheetSyncResponse> SyncAsync(List<GoogleSheetResponse> response, CancellationToken cancellationToken = default);
    }
}

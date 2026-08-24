using FinancialPortfolio.Models.Model.Response.MarketData;

namespace FinancialPortfolio.Business.Abstractions.IMarketData
{
    /// <summary>
    /// Applies NSE fundamentals on top of an already-saved price snapshot.
    /// </summary>
    public interface INseStockEnrichmentService
    {
        Task<MarketDataSyncResponse> EnrichAsync(DateOnly tradeDate, CancellationToken cancellationToken = default);
    }
}

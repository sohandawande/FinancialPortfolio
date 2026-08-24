using FinancialPortfolio.Business.Services.Fundamentals;

namespace FinancialPortfolio.Business.Abstractions.IFundamentals
{
    /// <summary>
    /// Provides normalized stock fundamental data for a specific NSE trading date.
    /// The implementation can be replaced without changing the synchronization workflow.
    /// </summary>
    public interface IFundamentalDataProvider
    {
        Task<IReadOnlyList<StockFundamentalData>> GetAsync(
            IReadOnlyCollection<string> symbols,
            DateOnly tradeDate,
            CancellationToken cancellationToken = default);
    }
}

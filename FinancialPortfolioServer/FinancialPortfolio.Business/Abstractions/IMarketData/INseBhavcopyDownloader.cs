using FinancialPortfolio.Business.Services.MarketData;

namespace FinancialPortfolio.Business.Abstractions.IMarketData
{
    /// <summary>
    /// Downloads official NSE market-data archives used by the price synchronization pipeline.
    /// Fundamental data is supplied through IFundamentalDataProvider.
    /// </summary>
    public interface INseBhavcopyDownloader
    {
        Task<(DateOnly TradeDate, IReadOnlyList<NseBhavcopyRow> Rows)?> DownloadAsync(
            DateOnly? preferredDate,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<NseWeek52Row>> DownloadWeek52Async(
            DateOnly tradeDate,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<NseIndexMemberRow>> DownloadIndexMembershipAsync(
            CancellationToken cancellationToken);
    }
}

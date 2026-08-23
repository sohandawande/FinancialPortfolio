namespace FinancialPortfolio.Business.Abstractions.ILogo
{
    public interface ILogoService
    {
        Task<string?> EnsureLogoAsync(string symbol, CancellationToken cancellationToken = default);
    }
}

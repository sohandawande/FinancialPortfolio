namespace FinancialPortfolio.Business.Abstractions.IValidation
{
    public interface IValidationService
    {
        Task ValidateAsync<T>(T model, CancellationToken cancellationToken = default);
    }
}

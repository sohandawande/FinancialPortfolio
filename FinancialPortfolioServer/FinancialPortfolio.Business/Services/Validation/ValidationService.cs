using FinancialPortfolio.Business.Abstractions.IValidation;
using FinancialPortfolio.Models.Common.Utilities;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using BusinessValidationException = FinancialPortfolio.Models.Common.Exceptions.ValidationException;

namespace FinancialPortfolio.Business.Services.Validation
{
    public sealed class ValidationService : IValidationService
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = Guard.AgainstNull(serviceProvider, nameof(serviceProvider));
        }

        public async Task ValidateAsync<T>(T model, CancellationToken cancellationToken = default)
        {
            var validator = _serviceProvider.GetService<IValidator<T>>();

            if (validator is null)
            {
                return;
            }

            var result = await validator.ValidateAsync(model, cancellationToken);

            if (!result.IsValid)
            {
                throw new BusinessValidationException(result.Errors.Select(x => x.ErrorMessage).ToArray());
            }
        }
    }
}

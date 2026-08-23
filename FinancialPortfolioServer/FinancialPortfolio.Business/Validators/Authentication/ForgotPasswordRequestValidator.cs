using FinancialPortfolio.Business.Common.Constants;
using FinancialPortfolio.Models.Model.Request.Authentication;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Authentication
{
    public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.EmailRequired)
                .EmailAddress()
                .WithMessage(ValidationMessageConstants.EmailInvalid);
        }
    }
}

using FinancialPortfolio.Business.Common.Constants;
using FinancialPortfolio.Models.Model.Request.Authentication;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Authentication
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.AccessTokenRequired);

            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.RefreshTokenRequired);
        }
    }
}

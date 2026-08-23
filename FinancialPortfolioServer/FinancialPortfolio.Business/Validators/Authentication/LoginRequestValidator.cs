using FinancialPortfolio.Business.Common.Constants;
using FinancialPortfolio.Models.Model.Request.Authentication;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Authentication
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.LoginId)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.LoginIdRequired);

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.PasswordRequired);
        }
    }
}

using FinancialPortfolio.Business.Common.Constants;
using FinancialPortfolio.Models.Model.Request.Authentication;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Authentication
{
    public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.CurrentPasswordRequired);

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.NewPasswordRequired)
                .MinimumLength(8)
                .WithMessage(ValidationMessageConstants.PasswordMinLength)
                .Matches(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$")
                .WithMessage(ValidationMessageConstants.PasswordComplexity)
                .NotEqual(x => x.CurrentPassword)
                .WithMessage(ValidationMessageConstants.NewPasswordSameAsCurrent);

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.ConfirmPasswordRequired)
                .Equal(x => x.NewPassword)
                .WithMessage(ValidationMessageConstants.PasswordsDoNotMatch);
        }
    }
}

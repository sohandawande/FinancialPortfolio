using FinancialPortfolio.Business.Common.Constants;
using FinancialPortfolio.Models.Model.Request.Authentication;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Authentication
{
    public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.EmailRequired)
                .EmailAddress()
                .WithMessage(ValidationMessageConstants.EmailInvalid);

            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.ResetTokenRequired);

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.PasswordRequired)
                .MinimumLength(8)
                .WithMessage(ValidationMessageConstants.PasswordMinLength)
                .Matches(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$")
                .WithMessage(ValidationMessageConstants.PasswordComplexity);

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.ConfirmPasswordRequired)
                .Equal(x => x.Password)
                .WithMessage(ValidationMessageConstants.PasswordsDoNotMatch);
        }
    }
}

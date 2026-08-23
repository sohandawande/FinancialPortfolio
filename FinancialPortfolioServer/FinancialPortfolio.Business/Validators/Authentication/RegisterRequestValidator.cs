using FinancialPortfolio.Business.Common.Constants;
using FinancialPortfolio.Models.Model.Request.Authentication;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Authentication
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.UserNameRequired)
                .MinimumLength(3)
                .WithMessage(ValidationMessageConstants.UserNameFormat)
                .MaximumLength(50)
                .WithMessage(ValidationMessageConstants.UserNameFormat)
                .Matches(@"^[a-zA-Z0-9._-]+$")
                .WithMessage(ValidationMessageConstants.UserNameFormat);

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.EmailRequired)
                .EmailAddress()
                .WithMessage(ValidationMessageConstants.EmailInvalid);

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

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.FirstNameRequired)
                .Matches(@"^[A-Za-z ]{2,100}$")
                .WithMessage(ValidationMessageConstants.FirstNameFormat);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.LastNameRequired)
                .Matches(@"^[A-Za-z ]{2,100}$")
                .WithMessage(ValidationMessageConstants.LastNameFormat);

            RuleFor(x => x.MobileNumber)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.MobileRequired)
                .Matches(@"^[0-9]{10}$")
                .WithMessage(ValidationMessageConstants.MobileInvalid);
        }
    }
}

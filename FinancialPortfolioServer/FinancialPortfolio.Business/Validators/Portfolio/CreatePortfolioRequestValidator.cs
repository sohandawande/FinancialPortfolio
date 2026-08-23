using FinancialPortfolio.Business.Common.Constants;
using FinancialPortfolio.Models.Model.Request.Portfolio;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Portfolio
{
    public class CreatePortfolioRequestValidator : AbstractValidator<CreatePortfolioRequest>
    {
        public CreatePortfolioRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.PortfolioNameRequired)
                .MaximumLength(100)
                .WithMessage(ValidationMessageConstants.PortfolioNameMax);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage(ValidationMessageConstants.PortfolioDescriptionMax)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}

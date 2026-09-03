using FinancialPortfolio.Models.Model.Request.Wealth;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Wealth
{
    public sealed class UpsertInsurancePolicyRequestValidator : AbstractValidator<UpsertInsurancePolicyRequest>
    {
        public UpsertInsurancePolicyRequestValidator()
        {
            RuleFor(x => x.InsurerName).NotEmpty().MaximumLength(120).WithMessage("Insurer is required.");
            RuleFor(x => x.PolicyNumber).NotEmpty().MaximumLength(60).WithMessage("Policy number is required.");
            RuleFor(x => x.PlanName).NotEmpty().MaximumLength(200).WithMessage("Plan name is required.");
            RuleFor(x => x.SumAssured).GreaterThan(0).WithMessage("Sum assured must be greater than zero.");
            RuleFor(x => x.PremiumAmount).GreaterThanOrEqualTo(0).WithMessage("Premium cannot be negative.");
            RuleFor(x => x.PremiumPayingTermYears).GreaterThanOrEqualTo(0).WithMessage("Premium paying term is invalid.");
            RuleFor(x => x.PolicyTermYears).GreaterThan(0).WithMessage("Policy term must be at least 1 year.");
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("Start date is required.");
            RuleFor(x => x.PremiumsPaid).GreaterThanOrEqualTo(0).WithMessage("Premiums paid cannot be negative.");
            RuleFor(x => x.ExpectedMaturityAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.ExpectedMaturityAmount.HasValue)
                .WithMessage("Expected maturity amount cannot be negative.");
            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }
}

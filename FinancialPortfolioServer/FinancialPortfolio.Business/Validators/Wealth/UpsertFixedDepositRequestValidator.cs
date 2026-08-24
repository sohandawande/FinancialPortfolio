using FinancialPortfolio.Models.Model.Request.Wealth;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Wealth
{
    public class UpsertFixedDepositRequestValidator : AbstractValidator<UpsertFixedDepositRequest>
    {
        public UpsertFixedDepositRequestValidator()
        {
            RuleFor(x => x.BankName).NotEmpty().MaximumLength(120);
            RuleFor(x => x.AccountRef).MaximumLength(60);
            RuleFor(x => x.Principal).GreaterThan(0);
            RuleFor(x => x.InterestRate).InclusiveBetween(0, 40);
            RuleFor(x => x.TenureMonths).InclusiveBetween(1, 360);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }
}

using FinancialPortfolio.Models.Model.Request.Wealth;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Wealth
{
    public class UpsertRdInstallmentRequestValidator : AbstractValidator<UpsertRdInstallmentRequest>
    {
        public UpsertRdInstallmentRequestValidator()
        {
            RuleFor(x => x.InstallmentNumber).GreaterThan(0);
            RuleFor(x => x.DueDate).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.FromBankName).MaximumLength(120);
            RuleFor(x => x.FromAccountNumber).MaximumLength(40);
            RuleFor(x => x.FromIfsc).MaximumLength(15);
            RuleFor(x => x.TransactionReference).MaximumLength(80);
            RuleFor(x => x.PenaltyAmount).GreaterThanOrEqualTo(0).When(x => x.PenaltyAmount.HasValue);
            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }
}

using FinancialPortfolio.Business.Common.Constants;
using FinancialPortfolio.Models.Model.Request.Portfolio;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Portfolio
{
    public class AddDividendRequestValidator : AbstractValidator<AddDividendRequest>
    {
        public AddDividendRequestValidator()
        {
            RuleFor(x => x.StockId)
                .GreaterThan(0)
                .WithMessage(ValidationMessageConstants.StockRequired);

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage(ValidationMessageConstants.QuantityMin);

            RuleFor(x => x)
                .Must(x => x.PerShareAmount > 0 || (x.Amount.HasValue && x.Amount.Value > 0))
                .WithMessage(ValidationMessageConstants.DividendAmountRequired);

            RuleFor(x => x.PerShareAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidationMessageConstants.PerShareInvalid)
                .PrecisionScale(18, 4, false)
                .WithMessage("Per-share amount can have at most 4 decimal places.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage(ValidationMessageConstants.AmountInvalid)
                .When(x => x.Amount.HasValue);

            RuleFor(x => x.DividendDate)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.DividendDateRequired)
                .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
                .WithMessage(ValidationMessageConstants.DividendDateFuture);

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage(ValidationMessageConstants.NotesMax)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));
        }
    }
}

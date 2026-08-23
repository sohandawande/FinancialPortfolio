using FinancialPortfolio.Business.Common.Constants;
using FinancialPortfolio.Models.Model.Request.Portfolio;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Portfolio
{
    public class UpdateHoldRequestValidator : AbstractValidator<UpdateHoldRequest>
    {
        public UpdateHoldRequestValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage(ValidationMessageConstants.QuantityMin);

            RuleFor(x => x.PurchasePrice)
                .GreaterThan(0)
                .WithMessage(ValidationMessageConstants.PurchasePriceMin)
                .PrecisionScale(18, 4, false)
                .WithMessage(ValidationMessageConstants.PurchasePriceScale);

            RuleFor(x => x.PurchaseDate)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.PurchaseDateRequired)
                .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
                .WithMessage(ValidationMessageConstants.PurchaseDateFuture);

            RuleFor(x => x.Exchange)
                .IsInEnum()
                .WithMessage(ValidationMessageConstants.ExchangeInvalid);

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage(ValidationMessageConstants.NotesMax)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));
        }
    }
}

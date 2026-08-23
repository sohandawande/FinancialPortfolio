using FinancialPortfolio.Business.Common.Constants;
using FinancialPortfolio.Models.Model.Request.Portfolio;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Portfolio
{
    public class UpdateSoldRequestValidator : AbstractValidator<UpdateSoldRequest>
    {
        public UpdateSoldRequestValidator()
        {
            RuleFor(x => x.SellQuantity)
                .GreaterThan(0)
                .WithMessage(ValidationMessageConstants.SellQuantityMin);

            RuleFor(x => x.SellPrice)
                .GreaterThan(0)
                .WithMessage(ValidationMessageConstants.SellPriceMin)
                .PrecisionScale(18, 4, false)
                .WithMessage(ValidationMessageConstants.SellPriceScale);

            RuleFor(x => x.SoldDate)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.SoldDateRequired)
                .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
                .WithMessage(ValidationMessageConstants.SoldDateFuture);

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage(ValidationMessageConstants.NotesMax)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));
        }
    }
}

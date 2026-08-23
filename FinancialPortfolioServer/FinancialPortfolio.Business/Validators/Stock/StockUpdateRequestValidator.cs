using FinancialPortfolio.Business.Common.Constants;
using FinancialPortfolio.Models.Model.Request.Stock;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Stock
{
    public class StockUpdateRequestValidator : AbstractValidator<StockUpdateRequest>
    {
        public StockUpdateRequestValidator()
        {
            RuleFor(x => x.Symbol)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.SymbolRequired)
                .MaximumLength(50)
                .WithMessage(ValidationMessageConstants.SymbolMax);

            RuleFor(x => x.CompanyName)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.CompanyNameRequired)
                .MaximumLength(250)
                .WithMessage(ValidationMessageConstants.CompanyNameMax);

            RuleFor(x => x.Industry)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.IndustryRequired)
                .MaximumLength(50)
                .WithMessage("Industry cannot exceed 50 characters.");

            RuleFor(x => x.ISINCode)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.IsinRequired)
                .MaximumLength(50)
                .WithMessage("ISIN cannot exceed 50 characters.");

            RuleFor(x => x.Series)
                .NotEmpty()
                .WithMessage(ValidationMessageConstants.SeriesRequired)
                .MaximumLength(20)
                .WithMessage("Series cannot exceed 20 characters.");

            RuleFor(x => x.CurrentPrice)
                .GreaterThan(0)
                .WithMessage(ValidationMessageConstants.CurrentPriceMin)
                .PrecisionScale(18, 2, false)
                .WithMessage("Current price can have at most 2 decimal places.");

            RuleFor(x => x.MarketCap)
                .GreaterThan(0)
                .WithMessage(ValidationMessageConstants.MarketCapMin);
        }
    }
}

using FinancialPortfolio.Models.Model.Request.Wealth;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.Wealth
{
    public class UpsertMutualFundRequestValidator : AbstractValidator<UpsertMutualFundRequest>
    {
        public UpsertMutualFundRequestValidator()
        {
            RuleFor(x => x.SchemeName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Amc).NotEmpty().MaximumLength(120);
            RuleFor(x => x.FolioNumber).MaximumLength(60);
            RuleFor(x => x.SchemeCode).GreaterThan(0).When(x => x.SchemeCode.HasValue);
            RuleFor(x => x.Units).GreaterThan(0);
            RuleFor(x => x.AverageNav).GreaterThan(0);
            RuleFor(x => x.CurrentNav).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PurchaseDate).NotEmpty();
            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }
}

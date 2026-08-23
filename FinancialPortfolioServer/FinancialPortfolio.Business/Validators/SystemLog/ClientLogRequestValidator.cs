using FinancialPortfolio.Models.Model.Request.SystemLog;
using FluentValidation;

namespace FinancialPortfolio.Business.Validators.SystemLog
{
    public class ClientLogRequestValidator : AbstractValidator<ClientLogRequest>
    {
        public ClientLogRequestValidator()
        {
            RuleFor(x => x.Level)
            .IsInEnum()
            .WithMessage("Invalid log level.");

            RuleFor(x => x.Category)
                .NotEmpty()
                .WithMessage("Category is required.")
                .MaximumLength(100)
                .WithMessage("Category must not exceed 100 characters.");

            RuleFor(x => x.Method)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Method))
                .WithMessage("Method must not exceed 200 characters.");

            RuleFor(x => x.Message)
                .NotEmpty()
                .WithMessage("Message is required.")
                .MaximumLength(4000)
                .WithMessage("Message must not exceed 4000 characters.");

            RuleFor(x => x.Exception)
                .MaximumLength(8000)
                .When(x => !string.IsNullOrWhiteSpace(x.Exception))
                .WithMessage("Exception must not exceed 8000 characters.");

            RuleFor(x => x.StackTrace)
                .MaximumLength(8000)
                .When(x => !string.IsNullOrWhiteSpace(x.StackTrace))
                .WithMessage("StackTrace must not exceed 8000 characters.");

            RuleFor(x => x.PageUrl)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.PageUrl))
                .WithMessage("PageUrl must not exceed 500 characters.");

            RuleFor(x => x.UserAgent)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.UserAgent))
                .WithMessage("UserAgent must not exceed 500 characters.");
        }
    }
}

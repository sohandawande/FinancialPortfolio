using FinancialPortfolio.Business.Abstractions.IEmail;
using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Utilities;

namespace FinancialPortfolio.Business.Services.Email
{
    public sealed class EmailTemplateService : IEmailTemplateService
    {
        private readonly string _templateRoot;
        private readonly IApplicationLoggerService _logger;

        public EmailTemplateService(IApplicationLoggerService logger)
        {
            _templateRoot = Path.Combine(AppContext.BaseDirectory, "EmailTemplates");
            _logger = Guard.AgainstNull(logger, nameof(logger));
        }

        public async Task<string> RenderAsync(string templateName, IDictionary<string, string> values, CancellationToken cancellationToken = default)
        {
            try
            {
                var layoutPath = Path.Combine(_templateRoot, "_layout.html");
                var templatePath = Path.Combine(_templateRoot, $"{templateName}.html");

                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"Email template not found: {templatePath}", templatePath);
                }

                var body = await File.ReadAllTextAsync(templatePath, cancellationToken);
                var layout = File.Exists(layoutPath) ? await File.ReadAllTextAsync(layoutPath, cancellationToken) : "{{Body}}";

                body = Apply(body, values);
                var html = layout.Replace("{{Body}}", body, StringComparison.OrdinalIgnoreCase);
                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Rendered email template: {templateName}", cancellationToken: cancellationToken);
                return Apply(html, values);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        private static string Apply(string content, IDictionary<string, string> values)
        {
            foreach (var pair in values)
            {
                content = content.Replace(
                    $"{{{{{pair.Key}}}}}",
                    pair.Value ?? string.Empty,
                    StringComparison.OrdinalIgnoreCase);
            }

            return content;
        }
    }
}

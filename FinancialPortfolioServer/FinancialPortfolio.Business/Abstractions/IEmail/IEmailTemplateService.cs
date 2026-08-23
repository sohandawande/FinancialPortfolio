namespace FinancialPortfolio.Business.Abstractions.IEmail
{
    public interface IEmailTemplateService
    {
        Task<string> RenderAsync(string templateName, IDictionary<string, string> values, CancellationToken cancellationToken = default);
    }
}

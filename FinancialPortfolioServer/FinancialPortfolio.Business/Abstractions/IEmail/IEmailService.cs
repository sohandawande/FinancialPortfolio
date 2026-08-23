namespace FinancialPortfolio.Business.Abstractions.IEmail
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);

        Task SendPasswordResetAsync(string toEmail, string resetToken, string? userName = null, CancellationToken cancellationToken = default);

        Task SendRegistrationPendingAsync(string toEmail, string fullName, CancellationToken cancellationToken = default);

        Task SendAccountApprovedAsync(string toEmail, string fullName, IEnumerable<string> roles, CancellationToken cancellationToken = default);

        Task SendRolesUpdatedAsync(string toEmail, string fullName, IEnumerable<string> roles, CancellationToken cancellationToken = default);

        Task SendAccountActivatedAsync(string toEmail, string fullName, CancellationToken cancellationToken = default);

        Task SendAccountDeactivatedAsync(string toEmail, string fullName, CancellationToken cancellationToken = default);
    }
}

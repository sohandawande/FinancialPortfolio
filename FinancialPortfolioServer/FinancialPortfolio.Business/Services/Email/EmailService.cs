namespace FinancialPortfolio.Business.Services.Email
{
    using FinancialPortfolio.Business.Abstractions.IEmail;
    using FinancialPortfolio.Business.Abstractions.ILogger;
    using FinancialPortfolio.Business.Common.Helpers;
    using FinancialPortfolio.Models.Common.Enums;
    using FinancialPortfolio.Models.Common.Utilities;
    using FinancialPortfolio.Models.Model.Settings;
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Microsoft.Extensions.Options;
    using MimeKit;

    /// <summary>
    /// Defines the <see cref="EmailService" />
    /// </summary>
    public sealed class EmailService : IEmailService
    {
        /// <summary>
        /// Defines the _settings
        /// </summary>
        private readonly EmailSettings _settings;

        /// <summary>
        /// Defines the _templates
        /// </summary>
        private readonly IEmailTemplateService _templates;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly IApplicationLoggerService _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailService"/> class.
        /// </summary>
        /// <param name="options">The options<see cref="IOptions{EmailSettings}"/></param>
        /// <param name="templates">The templates<see cref="IEmailTemplateService"/></param>
        /// <param name="logger">The logger<see cref="IApplicationLoggerService"/></param>
        public EmailService(IOptions<EmailSettings> options, IEmailTemplateService templates, IApplicationLoggerService logger)
        {
            _settings = Guard.AgainstNull(options?.Value, nameof(options.Value));
            _templates = Guard.AgainstNull(templates, nameof(templates));
            _logger = Guard.AgainstNull(logger, nameof(logger));
        }

        /// <summary>
        /// The SendAsync
        /// </summary>
        /// <param name="toEmail">The toEmail<see cref="string"/></param>
        /// <param name="subject">The subject<see cref="string"/></param>
        /// <param name="htmlBody">The htmlBody<see cref="string"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            try
            {
                var provider = _settings.GetActive();

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(provider.FromName, provider.FromEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = htmlBody };

                using var client = new SmtpClient();

                await client.ConnectAsync(provider.Host, provider.Port, provider.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, cancellationToken);

                if (!string.IsNullOrWhiteSpace(provider.UserName))
                {
                    await client.AuthenticateAsync(provider.UserName, provider.Password, cancellationToken);
                }

                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Email sent to {toEmail} successfully.", cancellationToken);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// The SendPasswordResetAsync
        /// </summary>
        /// <param name="toEmail">The toEmail<see cref="string"/></param>
        /// <param name="resetToken">The resetToken<see cref="string"/></param>
        /// <param name="userName">The userName<see cref="string?"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SendPasswordResetAsync(string toEmail, string resetToken, string? userName = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var link = $"{_settings.ResetPasswordUrl}?email={Uri.EscapeDataString(toEmail)}&token={Uri.EscapeDataString(resetToken)}";

                var values = Base("Reset your password");
                values["ResetLink"] = link;
                values["UserName"] = userName ?? string.Empty;
                values["UserNameSuffix"] = string.IsNullOrWhiteSpace(userName) ? "" : $" {userName}";

                var html = await _templates.RenderAsync("password-reset", values, cancellationToken);
                await SendAsync(toEmail, "Reset your password", html, cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Reset password email sent to {toEmail} successfully.", cancellationToken);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// The SendRegistrationPendingAsync
        /// </summary>
        /// <param name="toEmail">The toEmail<see cref="string"/></param>
        /// <param name="fullName">The fullName<see cref="string"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SendRegistrationPendingAsync(string toEmail, string fullName, CancellationToken cancellationToken = default)
        {
            try
            {
                var values = Base("Registration received");
                values["UserName"] = fullName;
                values["LoginLink"] = _settings.LoginUrl;

                var html = await _templates.RenderAsync("registration-pending", values, cancellationToken);
                await SendAsync(toEmail, "Registration received — pending approval", html, cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Registration pending email sent to {toEmail} successfully.", cancellationToken);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// The SendAccountApprovedAsync
        /// </summary>
        /// <param name="toEmail">The toEmail<see cref="string"/></param>
        /// <param name="fullName">The fullName<see cref="string"/></param>
        /// <param name="roles">The roles<see cref="IEnumerable{string}"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SendAccountApprovedAsync(string toEmail, string fullName, IEnumerable<string> roles, CancellationToken cancellationToken = default)
        {
            try
            {
                var values = Base("Account approved");
                values["UserName"] = fullName;
                values["Roles"] = FormatRoles(roles);
                values["LoginLink"] = _settings.LoginUrl;

                var html = await _templates.RenderAsync("account-approved", values, cancellationToken);
                await SendAsync(toEmail, "Your account has been approved", html, cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Account approved email sent to {toEmail} successfully.", cancellationToken);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// The SendRolesUpdatedAsync
        /// </summary>
        /// <param name="toEmail">The toEmail<see cref="string"/></param>
        /// <param name="fullName">The fullName<see cref="string"/></param>
        /// <param name="roles">The roles<see cref="IEnumerable{string}"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SendRolesUpdatedAsync(string toEmail, string fullName, IEnumerable<string> roles, CancellationToken cancellationToken = default)
        {
            try
            {
                var values = Base("Roles updated");
                values["UserName"] = fullName;
                values["Roles"] = FormatRoles(roles);
                values["LoginLink"] = _settings.LoginUrl;

                var html = await _templates.RenderAsync("roles-updated", values, cancellationToken);
                await SendAsync(toEmail, "Your roles have been updated", html, cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Roles updated email sent to {toEmail} successfully.", cancellationToken);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// The SendAccountActivatedAsync
        /// </summary>
        /// <param name="toEmail">The toEmail<see cref="string"/></param>
        /// <param name="fullName">The fullName<see cref="string"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SendAccountActivatedAsync(string toEmail, string fullName, CancellationToken cancellationToken = default)
        {
            try
            {
                var values = Base("Account activated");
                values["UserName"] = fullName;
                values["LoginLink"] = _settings.LoginUrl;

                var html = await _templates.RenderAsync("account-activated", values, cancellationToken);
                await SendAsync(toEmail, "Your account is active", html, cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Account activated email sent to {toEmail} successfully.", cancellationToken);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// The SendAccountDeactivatedAsync
        /// </summary>
        /// <param name="toEmail">The toEmail<see cref="string"/></param>
        /// <param name="fullName">The fullName<see cref="string"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SendAccountDeactivatedAsync(string toEmail, string fullName, CancellationToken cancellationToken = default)
        {
            try
            {
                var values = Base("Account deactivated");
                values["UserName"] = fullName;
                values["LoginLink"] = _settings.LoginUrl;

                var html = await _templates.RenderAsync("account-deactivated", values, cancellationToken);
                await SendAsync(toEmail, "Your account has been deactivated", html, cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Account deactivated email sent to {toEmail} successfully.", cancellationToken);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// The FormatRoles
        /// </summary>
        /// <param name="roles">The roles<see cref="IEnumerable{string}?"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string FormatRoles(IEnumerable<string>? roles)
        {
            var list = roles?.Where(r => !string.IsNullOrWhiteSpace(r)).ToList() ?? [];
            return list.Count == 0 ? "User" : string.Join(", ", list);
        }

        /// <summary>
        /// The Base
        /// </summary>
        /// <param name="subject">The subject<see cref="string"/></param>
        /// <returns>The <see cref="Dictionary{string, string}"/></returns>
        private static Dictionary<string, string> Base(string subject) =>
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Subject"] = subject,
                ["Year"] = DateTime.UtcNow.Year.ToString()
            };
    }
}

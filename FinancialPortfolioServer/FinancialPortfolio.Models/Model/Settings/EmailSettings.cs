namespace FinancialPortfolio.Models.Model.Settings
{
    public sealed class EmailSettings
    {
        public const string SectionName = "Email";

        /// <summary>Mailtrap | Gmail | Brevo</summary>
        public string ActiveProvider { get; set; } = "Mailtrap";

        public string ResetPasswordUrl { get; set; } = "http://localhost:4200/reset-password";
        public string LoginUrl { get; set; } = "http://localhost:4200/login";

        public EmailProviderSettings Mailtrap { get; set; } = new();
        public EmailProviderSettings Gmail { get; set; } = new();
        public EmailProviderSettings Brevo { get; set; } = new();

        public EmailProviderSettings GetActive()
        {
            return ActiveProvider?.Trim().ToLowerInvariant() switch
            {
                "gmail" => Gmail,
                "brevo" => Brevo,
                _ => Mailtrap
            };
        }
    }
}

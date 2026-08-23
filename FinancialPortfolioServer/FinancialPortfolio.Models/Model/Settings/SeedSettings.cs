namespace FinancialPortfolio.Models.Model.Settings
{
    public sealed class SeedSettings
    {
        public const string SectionName = "Seed";

        /// <summary>
        /// When false, admin user seeding is skipped.
        /// </summary>
        public bool Enabled { get; set; }

        public string AdminEmail { get; set; } = "admin@financialportfolio.local";

        public string AdminPassword { get; set; } = string.Empty;

        /// <summary>
        /// Identity UserName. Prefer same as email (matches registration style).
        /// </summary>
        public string AdminUserName { get; set; } = "System_Admin";

        public string AdminFirstName { get; set; } = "System";

        public string AdminLastName { get; set; } = "Admin";

        public string AdminUserCode { get; set; } = "FP0001";

        public string AdminMobileNumber { get; set; } = "0000000000";
    }
}

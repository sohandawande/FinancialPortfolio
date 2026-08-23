namespace FinancialPortfolio.Data.Identity
{
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// Defines the <see cref="ApplicationUser" />
    /// </summary>
    public class ApplicationUser : IdentityUser<Guid>
    {
        // Keep this class minimal.
        // Only authentication-related properties should go here.

        /// <summary>
        /// Gets or sets the UserRoles
        /// </summary>
        public virtual ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = new List<IdentityUserRole<Guid>>();
    }
}
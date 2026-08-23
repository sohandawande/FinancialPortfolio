namespace FinancialPortfolio.Data.Identity
{
    using Microsoft.AspNetCore.Identity;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="ApplicationRole" />
    /// </summary>
    public class ApplicationRole : IdentityRole<Guid>
    {
        /// <summary>
        /// Gets or sets the UserRoles
        /// </summary>
        public virtual ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = new List<IdentityUserRole<Guid>>();
    }
}

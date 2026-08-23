using FinancialPortfolio.Data.Identity;
using FinancialPortfolio.Models.Common.Constants;
using Microsoft.AspNetCore.Identity;

namespace FinancialPortfolio.Data.Seed
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager)
        {
            var roles = new[]
            {
                RoleConstants.Pending,
                RoleConstants.Admin,
                RoleConstants.User,
                RoleConstants.PortfolioManager,
                RoleConstants.Trader,
                RoleConstants.Viewer
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Id = Guid.NewGuid(),
                        Name = role,
                        NormalizedName = role.ToUpper()
                    });
                }
            }
        }
    }
}

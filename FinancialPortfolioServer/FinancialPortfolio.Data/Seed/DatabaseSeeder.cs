using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Data.Identity;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinancialPortfolio.Data.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var roleManager = sp.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var seedSettings = sp.GetRequiredService<IOptions<SeedSettings>>().Value;
            var timeProvider = sp.GetRequiredService<TimeProvider>();

            logger.LogInformation("Applying EF Core migrations...");
            await db.Database.MigrateAsync(cancellationToken);

            logger.LogInformation("Seeding roles...");
            await RoleSeeder.SeedAsync(roleManager);

            logger.LogInformation("Seeding admin user (if enabled)...");
            await AdminUserSeeder.SeedAsync(
                userManager,
                roleManager,
                db,
                seedSettings,
                timeProvider,
                logger,
                cancellationToken);

            logger.LogInformation("Database seeding completed.");
        }
    }
}

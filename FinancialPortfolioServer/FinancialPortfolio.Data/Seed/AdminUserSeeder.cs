using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Data.Entities.AppUser;
using FinancialPortfolio.Data.Identity;
using FinancialPortfolio.Models.Common.Constants;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinancialPortfolio.Data.Seed
{
    public static class AdminUserSeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            SeedSettings settings,
            TimeProvider timeProvider,
            ILogger? logger = null,
            CancellationToken cancellationToken = default)
        {
            if (!settings.Enabled)
            {
                logger?.LogInformation("Admin user seeding is disabled (Seed:Enabled = false).");
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.AdminEmail) ||
                string.IsNullOrWhiteSpace(settings.AdminPassword))
            {
                logger?.LogWarning("Admin seeding skipped: AdminEmail or AdminPassword is empty.");
                return;
            }

            var email = settings.AdminEmail.Trim();

            // Identity UserName — prefer configured value, else email
            var userName = string.IsNullOrWhiteSpace(settings.AdminUserName)
                ? email
                : settings.AdminUserName.Trim();

            // Business user code (e.g. FP0001)
            var userCode = string.IsNullOrWhiteSpace(settings.AdminUserCode)
                ? "FP0001"
                : settings.AdminUserCode.Trim();

            var firstName = string.IsNullOrWhiteSpace(settings.AdminFirstName)
                ? "System"
                : settings.AdminFirstName.Trim();

            var lastName = string.IsNullOrWhiteSpace(settings.AdminLastName)
                ? "Admin"
                : settings.AdminLastName.Trim();

            // FIX: build full name correctly (parentheses / explicit join)
            var fullName = $"{firstName} {lastName}".Trim();

            var mobile = string.IsNullOrWhiteSpace(settings.AdminMobileNumber)
                ? "0000000000"
                : settings.AdminMobileNumber.Trim();

            // Already seeded by email?
            var existingByEmail = await userManager.FindByEmailAsync(email);
            if (existingByEmail is not null)
            {
                logger?.LogInformation("Admin identity user already exists: {Email}", email);
                await EnsureAdminRoleAndProfileAsync(
                    userManager,
                    roleManager,
                    context,
                    existingByEmail,
                    userCode,
                    firstName,
                    lastName,
                    fullName,
                    mobile,
                    timeProvider,
                    logger,
                    cancellationToken);
                return;
            }

            // Already seeded by user code?
            var existingProfile = await context.AppUsers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserCode == userCode && !x.IsDeleted, cancellationToken);

            if (existingProfile is not null)
            {
                logger?.LogInformation("Admin portfolio profile already exists for UserCode: {UserCode}", userCode);
                return;
            }

            if (!await roleManager.RoleExistsAsync(RoleConstants.Admin))
            {
                logger?.LogError("Admin role '{Role}' does not exist. Run RoleSeeder first.", RoleConstants.Admin);
                throw new InvalidOperationException(
                    $"Role '{RoleConstants.Admin}' was not found. Seed roles before admin user.");
            }

            var utcNow = timeProvider.GetUtcNow().UtcDateTime;

            var identityUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                NormalizedUserName = userName.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                EmailConfirmed = true,
                PhoneNumber = mobile,
                LockoutEnabled = false
            };

            var createResult = await userManager.CreateAsync(identityUser, settings.AdminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger?.LogError("Failed to create admin identity user: {Errors}", errors);
                throw new InvalidOperationException($"Failed to seed admin user: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(identityUser, RoleConstants.Admin);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                logger?.LogError("Failed to assign Admin role: {Errors}", errors);
                throw new InvalidOperationException($"Failed to assign Admin role: {errors}");
            }

            var appUser = new AppUserEntity
            {
                IdentityUserId = identityUser.Id,
                UserCode = userCode,
                FirstName = firstName,
                LastName = lastName,
                FullName = fullName,
                MobileNumber = mobile,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = identityUser.Id,
                ModifiedBy = identityUser.Id,
                CreatedDate = utcNow,
                ModifiedDate = utcNow
            };

            context.AppUsers.Add(appUser);
            await context.SaveChangesAsync(cancellationToken);

            logger?.LogInformation(
                "Seeded admin user. Email={Email}, UserName={UserName}, UserCode={UserCode}, FullName={FullName}, Role={Role}",
                email,
                userName,
                userCode,
                fullName,
                RoleConstants.Admin);
        }

        private static async Task EnsureAdminRoleAndProfileAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            ApplicationUser identityUser,
            string userCode,
            string firstName,
            string lastName,
            string fullName,
            string mobile,
            TimeProvider timeProvider,
            ILogger? logger,
            CancellationToken cancellationToken)
        {
            if (!await userManager.IsInRoleAsync(identityUser, RoleConstants.Admin))
            {
                if (await roleManager.RoleExistsAsync(RoleConstants.Admin))
                {
                    await userManager.AddToRoleAsync(identityUser, RoleConstants.Admin);
                    logger?.LogInformation("Assigned missing Admin role to {Email}", identityUser.Email);
                }
            }

            var profile = await context.AppUsers
                .FirstOrDefaultAsync(
                    x => x.IdentityUserId == identityUser.Id && !x.IsDeleted,
                    cancellationToken);

            if (profile is null)
            {
                var utcNow = timeProvider.GetUtcNow().UtcDateTime;
                context.AppUsers.Add(new AppUserEntity
                {
                    IdentityUserId = identityUser.Id,
                    UserCode = userCode,
                    FirstName = firstName,
                    LastName = lastName,
                    FullName = fullName,
                    MobileNumber = mobile,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = identityUser.Id,
                    ModifiedBy = identityUser.Id,
                    CreatedDate = utcNow,
                    ModifiedDate = utcNow
                });
                await context.SaveChangesAsync(cancellationToken);
                logger?.LogInformation(
                    "Created missing PortfolioUser profile for admin {Email} ({FullName}, {UserCode})",
                    identityUser.Email,
                    fullName,
                    userCode);
                return;
            }

            // Repair mismatched seed fields on existing profile
            var changed = false;
            if (!string.Equals(profile.UserCode, userCode, StringComparison.Ordinal))
            {
                profile.UserCode = userCode;
                changed = true;
            }
            if (!string.Equals(profile.FirstName, firstName, StringComparison.Ordinal))
            {
                profile.FirstName = firstName;
                changed = true;
            }
            if (!string.Equals(profile.LastName, lastName, StringComparison.Ordinal))
            {
                profile.LastName = lastName;
                changed = true;
            }
            if (!string.Equals(profile.FullName, fullName, StringComparison.Ordinal))
            {
                profile.FullName = fullName;
                changed = true;
            }
            if (!profile.IsActive)
            {
                profile.IsActive = true;
                changed = true;
            }

            if (changed)
            {
                profile.ModifiedDate = timeProvider.GetUtcNow().UtcDateTime;
                profile.ModifiedBy = identityUser.Id;
                await context.SaveChangesAsync(cancellationToken);
                logger?.LogInformation(
                    "Repaired admin PortfolioUser profile for {Email}: FullName={FullName}, UserCode={UserCode}",
                    identityUser.Email,
                    fullName,
                    userCode);
            }
        }
    }
}
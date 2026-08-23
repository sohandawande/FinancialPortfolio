using FinancialPortfolio.Business.Abstractions.IAppUser;
using FinancialPortfolio.Business.Abstractions.IEmail;
using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Abstractions.IValidation;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Data.Identity;
using FinancialPortfolio.Models.Abstractions.ICurrentUser;
using FinancialPortfolio.Models.Common.Constants;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Exceptions;
using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Request.User;
using FinancialPortfolio.Models.Model.Response.AppUser;
using FinancialPortfolio.Models.Model.Response.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinancialPortfolio.Business.Services.User
{
    public sealed class AppUserService : IAppUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IApplicationLoggerService _logger;
        private readonly IValidationService _validation;
        private readonly IEmailService _emailService;
        private readonly ICurrentUserService _currentUserService;

        public AppUserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context, IApplicationLoggerService logger, IValidationService validation, IEmailService emailService, ICurrentUserService currentUserService)
        {
            _userManager = Guard.AgainstNull(userManager, nameof(userManager));
            _roleManager = Guard.AgainstNull(roleManager, nameof(roleManager));
            _context = Guard.AgainstNull(context, nameof(context));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _validation = Guard.AgainstNull(validation, nameof(validation));
            _emailService = Guard.AgainstNull(emailService, nameof(emailService));
            _currentUserService = Guard.AgainstNull(currentUserService, nameof(currentUserService));
        }

        public async Task<ApiResponse<List<PendingUserResponse>>> GetPendingUsersAsync(CancellationToken cancellationToken)
        {
            try
            {
                var pendingUsers = await
            (
                from p in _context.AppUsers
                join u in _context.Users
                    on p.IdentityUserId equals u.Id
                join ur in _context.UserRoles
                    on u.Id equals ur.UserId
                join r in _context.Roles
                    on ur.RoleId equals r.Id
                where r.Name == RoleConstants.Pending
                select new PendingUserResponse
                {
                    IdentityUserId = u.Id,
                    UserId = p.Id,
                    UserCode = p.UserCode,
                    FullName = p.FullName,
                    Email = u.Email!,
                    CreatedDate = p.CreatedDate
                }
            ).ToListAsync(cancellationToken);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "Pending users list fetch successfully.", cancellationToken);
                return ResponseFactory.Success(pendingUsers, "Pending users list fetch successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }

        }

        public async Task<ApiResponse<bool>> ApproveUserAsync(Guid identityUserId, AssignRoleRequest request, CancellationToken cancellationToken)
        {
            await _validation.ValidateAsync(request, cancellationToken);
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var identityUser = await _userManager.FindByIdAsync(identityUserId.ToString());

                if (identityUser is null)
                {
                    throw new NotFoundException("User not found.");
                }

                if (request.Roles is null || request.Roles.Count == 0)
                {
                    throw new ValidationException("At least one role must be selected.");
                }

                // Validate all roles
                foreach (var role in request.Roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        throw new NotFoundException($"Role '{role}' not found.");
                    }
                }

                // Remove Pending role
                await _userManager.RemoveFromRoleAsync(identityUser, RoleConstants.Pending);

                // Assign selected roles
                var result = await _userManager.AddToRolesAsync(identityUser, request.Roles);

                if (!result.Succeeded)
                {
                    throw new ValidationException(result.Errors.Select(x => x.Description).ToList());
                }

                var appUser = await _context.AppUsers.FirstAsync(x => x.IdentityUserId == identityUserId, cancellationToken);

                appUser.IsActive = true;

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                try
                {
                    var fullName = CommonHelper.BuildFullName(appUser.FirstName, appUser.LastName);
                    await _emailService.SendAccountApprovedAsync(identityUser.Email!, fullName, request.Roles, cancellationToken);
                }
                catch (Exception emailEx)
                {
                    await _logger.LogWarningAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Email failed: {emailEx.Message}", cancellationToken);
                }

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"User '{identityUser.Email}' approved with roles: {string.Join(", ", request.Roles)}.", cancellationToken);

                return ResponseFactory.Success(true, "User approved successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<bool>> AssignRoleAsync(Guid identityUserId, AssignRoleRequest request, CancellationToken cancellationToken)
        {
            EnsureNotSelf(identityUserId, "change roles for");
            await _validation.ValidateAsync(request, cancellationToken);
            try
            {
                var user = await _userManager.FindByIdAsync(identityUserId.ToString());

                if (user is null)
                {
                    throw new NotFoundException("User not found.");
                }

                if (request.Roles is null || request.Roles.Count == 0)
                {
                    throw new ValidationException("At least one role must be selected.");
                }

                // Validate all requested roles
                foreach (var role in request.Roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        throw new NotFoundException($"Role '{role}' not found.");
                    }
                }

                // Remove all existing roles
                var existingRoles = await _userManager.GetRolesAsync(user);

                if (existingRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);

                    if (!removeResult.Succeeded)
                    {
                        throw new ValidationException(removeResult.Errors.Select(x => x.Description).ToList());
                    }
                }

                // Add new roles
                var addResult = await _userManager.AddToRolesAsync(user, request.Roles);

                if (!addResult.Succeeded)
                {
                    throw new ValidationException(addResult.Errors.Select(x => x.Description).ToList());
                }

                try
                {
                    // load portfolio user for name if needed
                    await _emailService.SendRolesUpdatedAsync(user.Email!, user.UserName ?? user.Email!, request.Roles, cancellationToken);
                }
                catch (Exception emailEx)
                {
                    await _logger.LogWarningAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Email failed: {emailEx.Message}", cancellationToken);
                }

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Roles changed to '{string.Join(", ", request.Roles)}' for '{user.Email}'.", cancellationToken);

                return ResponseFactory.Success(true, "Roles assigned successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }

        }

        public async Task<ApiResponse<bool>> ActivateUserAsync(Guid identityUserId, CancellationToken cancellationToken)
        {
            EnsureNotSelf(identityUserId, "activate");
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var user = await _context.AppUsers.FirstAsync(x => x.IdentityUserId == identityUserId, cancellationToken);

                user.IsActive = true;

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                try
                {
                    var identity = await _userManager.FindByIdAsync(identityUserId.ToString());
                    var fullName = CommonHelper.BuildFullName(user.FirstName, user.LastName);
                    if (identity?.Email is not null)
                    {
                        await _emailService.SendAccountActivatedAsync(identity.Email, fullName, cancellationToken);
                    }
                }
                catch (Exception emailEx)
                {
                    await _logger.LogWarningAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Email failed: {emailEx.Message}", cancellationToken);
                }

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "User Activated Successfully.", cancellationToken);

                return ResponseFactory.Success(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<bool>> DeactivateUserAsync(Guid identityUserId, CancellationToken cancellationToken)
        {
            EnsureNotSelf(identityUserId, "deactivate");
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var user = await _context.AppUsers.FirstAsync(x => x.IdentityUserId == identityUserId, cancellationToken);

                user.IsActive = false;

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                try
                {
                    var identity = await _userManager.FindByIdAsync(identityUserId.ToString());
                    var fullName = CommonHelper.BuildFullName(user.FirstName, user.LastName);
                    if (identity?.Email is not null)
                    {
                        await _emailService.SendAccountDeactivatedAsync(identity.Email, fullName, cancellationToken);
                    }
                }
                catch (Exception emailEx)
                {
                    await _logger.LogWarningAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Email failed: {emailEx.Message}", cancellationToken);
                }

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "User Deactivated Successfully.", cancellationToken);

                return ResponseFactory.Success(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }
        public async Task<ApiResponse<List<ManagedUserResponse>>> GetManagedUsersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var pendingRole = await _roleManager.FindByNameAsync(RoleConstants.Pending);
                var pendingRoleId = pendingRole?.Id;

                var rows = await (
                    from p in _context.AppUsers
                    join u in _context.Users on p.IdentityUserId equals u.Id
                    where !p.IsDeleted
                    select new
                    {
                        p.Id,
                        p.IdentityUserId,
                        p.UserCode,
                        p.FirstName,
                        p.LastName,
                        p.MobileNumber,
                        p.IsActive,
                        p.CreatedDate,
                        p.ModifiedDate,
                        u.Email
                    }
                ).ToListAsync(cancellationToken);

                var result = new List<ManagedUserResponse>();

                foreach (var row in rows)
                {
                    var identity = await _userManager.FindByIdAsync(row.IdentityUserId.ToString());
                    if (identity is null) continue;

                    var roles = (await _userManager.GetRolesAsync(identity)).ToList();

                    // Exclude pure pending registrations
                    if (roles.Count == 1 && roles.Contains(RoleConstants.Pending))
                        continue;

                    if (roles.Contains(RoleConstants.Pending) && roles.Count == 1)
                        continue;

                    // Skip users who still only have Pending
                    if (roles.All(r => r == RoleConstants.Pending))
                        continue;

                    result.Add(new ManagedUserResponse
                    {
                        IdentityUserId = row.IdentityUserId,
                        UserId = row.Id,
                        UserCode = row.UserCode,
                        FullName = $"{row.FirstName} {row.LastName}".Trim(),
                        Email = row.Email ?? string.Empty,
                        MobileNumber = row.MobileNumber,
                        IsActive = row.IsActive,
                        Roles = roles.Where(r => r != RoleConstants.Pending).ToList(),
                        CreatedDate = row.CreatedDate,
                        ModifiedDate = row.ModifiedDate
                    });
                }

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "Managed users list fetched.", cancellationToken);

                return ResponseFactory.Success(result, "Managed users list fetched.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<UserDetailResponse>> GetUserByIdAsync(
            Guid identityUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var row = await (
                    from p in _context.AppUsers
                    join u in _context.Users on p.IdentityUserId equals u.Id
                    where p.IdentityUserId == identityUserId && !p.IsDeleted
                    select new
                    {
                        p.Id,
                        p.IdentityUserId,
                        p.UserCode,
                        p.FirstName,
                        p.LastName,
                        p.MobileNumber,
                        p.IsActive,
                        p.CreatedDate,
                        p.ModifiedDate,
                        u.Email,
                        u.UserName,
                        u.EmailConfirmed
                    }
                ).FirstOrDefaultAsync(cancellationToken);

                if (row is null)
                {
                    throw new NotFoundException("User not found.");
                }

                var identity = await _userManager.FindByIdAsync(identityUserId.ToString());
                if (identity is null)
                {
                    throw new NotFoundException("Identity user not found.");
                }

                var roles = (await _userManager.GetRolesAsync(identity)).ToList();

                var detail = new UserDetailResponse
                {
                    IdentityUserId = row.IdentityUserId,
                    UserId = row.Id,
                    UserName = row.UserName ?? string.Empty,
                    UserCode = row.UserCode,
                    FirstName = row.FirstName,
                    LastName = row.LastName,
                    FullName = $"{row.FirstName} {row.LastName}".Trim(),
                    Email = row.Email ?? string.Empty,
                    MobileNumber = row.MobileNumber,
                    IsActive = row.IsActive,
                    EmailConfirmed = row.EmailConfirmed,
                    Roles = roles.Where(r => r != RoleConstants.Pending).ToList(),
                    CreatedDate = row.CreatedDate,
                    ModifiedDate = row.ModifiedDate
                };

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"User detail fetched for {detail.Email}.", cancellationToken);

                return ResponseFactory.Success(detail, "User detail fetched.");
            }
            catch (Exception ex) when (ex is not NotFoundException and not ValidationException)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }
        private void EnsureNotSelf(Guid identityUserId, string action)
        {
            if (!_currentUserService.IsAuthenticated)
                return;

            if (identityUserId == _currentUserService.IdentityUserId)
            {
                throw new ValidationException($"You cannot {action} your own account.");
            }
        }
    }
}

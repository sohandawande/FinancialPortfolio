using AutoMapper;
using FinancialPortfolio.Business.Abstractions.IAuthentication;
using FinancialPortfolio.Business.Abstractions.IEmail;
using FinancialPortfolio.Business.Abstractions.IJwtToken;
using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Abstractions.INotification;
using FinancialPortfolio.Business.Abstractions.IValidation;
using FinancialPortfolio.Business.Common.Constants;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Data.Entities.AppUser;
using FinancialPortfolio.Data.Entities.RefreshToken;
using FinancialPortfolio.Data.Identity;
using FinancialPortfolio.Models.Abstractions.ICurrentUser;
using FinancialPortfolio.Models.Common.Constants;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Exceptions;
using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Request.Authentication;
using FinancialPortfolio.Models.Model.Response.Authentication;
using FinancialPortfolio.Models.Model.Response.User;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FinancialPortfolio.Business.Services.Authentication
{
    public sealed class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;
        private readonly TimeProvider _timeProvider;
        private readonly IApplicationLoggerService _logger;
        private readonly IValidationService _validation;
        private readonly ICurrentUserService _currentUserService;
        private readonly IRealtimeNotificationService _realtimeNotificationService;
        private readonly IEmailService _emailService;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            IJwtTokenService jwtTokenService,
            IMapper mapper,
            IOptions<JwtSettings> jwtOptions,
            TimeProvider timeProvider,
            IApplicationLoggerService logger,
            IValidationService validation,
            ICurrentUserService currentUserService,
            IRealtimeNotificationService realtimeNotificationService,
            IEmailService emailService)
        {
            _userManager = Guard.AgainstNull(userManager, nameof(userManager));
            _roleManager = Guard.AgainstNull(roleManager, nameof(roleManager));
            _context = Guard.AgainstNull(context, nameof(context));
            _jwtTokenService = Guard.AgainstNull(jwtTokenService, nameof(jwtTokenService));
            _mapper = Guard.AgainstNull(mapper, nameof(mapper));
            _jwtSettings = Guard.AgainstNull(jwtOptions?.Value, nameof(jwtOptions));
            _timeProvider = Guard.AgainstNull(timeProvider, nameof(timeProvider));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _validation = Guard.AgainstNull(validation, nameof(validation));
            _currentUserService = Guard.AgainstNull(currentUserService, nameof(currentUserService));
            _realtimeNotificationService = Guard.AgainstNull(realtimeNotificationService, nameof(realtimeNotificationService));
            _emailService = Guard.AgainstNull(emailService, nameof(emailService));
        }

        public async Task<ApiResponse<LoginResponse>> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var email = request.Email.Trim();
                var userName = request.UserName.Trim();

                if (await _userManager.FindByEmailAsync(email) is not null)
                    throw new ConflictException(ValidationMessageConstants.EmailAlreadyExists);

                if (await _userManager.FindByNameAsync(userName) is not null)
                    throw new ConflictException("Username is already taken.");

                var userCode = await UserCodeGeneratorHelper.NextAsync(_context, cancellationToken);
                var fullName = CommonHelper.BuildFullName(request.FirstName, request.LastName);
                var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

                var identityUser = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = userName,
                    Email = email,
                    PhoneNumber = request.MobileNumber.Trim(),
                    EmailConfirmed = true
                };

                var identityResult = await _userManager.CreateAsync(identityUser, request.Password);
                if (!identityResult.Succeeded)
                {
                    await _logger.LogWarningAsync(
                        ApplicationLevelType.Business,
                        LogSourceHelper.Current(),
                        $"Registration failed for {email}: {string.Join(", ", identityResult.Errors.Select(x => x.Description))}",
                        cancellationToken);
                    throw new ValidationException(identityResult.Errors.Select(x => x.Description).ToList());
                }

                if (!await _roleManager.RoleExistsAsync(RoleConstants.Pending))
                    throw new NotFoundException(ValidationMessageConstants.DefaultRoleNotFound);

                var roleResult = await _userManager.AddToRoleAsync(identityUser, RoleConstants.Pending);
                if (!roleResult.Succeeded)
                    throw new ValidationException(roleResult.Errors.Select(x => x.Description).ToList());

                var appUser = _mapper.Map<AppUserEntity>(request);
                appUser.IdentityUserId = identityUser.Id;
                appUser.UserCode = userCode;
                appUser.FullName = fullName;
                appUser.IsActive = false;
                appUser.CreatedBy = identityUser.Id;
                appUser.ModifiedBy = identityUser.Id;
                appUser.CreatedDate = utcNow;
                appUser.ModifiedDate = utcNow;

                _context.AppUsers.Add(appUser);
                await _context.SaveChangesAsync(cancellationToken);

                _currentUserService.SetUnauthenticatedUserContext(
                    appUser.Id, identityUser.Id, identityUser.Email!);

                await transaction.CommitAsync(cancellationToken);

                await _logger.LogInformationAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    $"User {email} registered as '{userName}' with code {userCode}.",
                    cancellationToken);

                try
                {
                    await _emailService.SendRegistrationPendingAsync(email, fullName, cancellationToken);
                }
                catch (Exception emailEx)
                {
                    await _logger.LogErrorAsync(
                        ApplicationLevelType.Business,
                        LogSourceHelper.Current(),
                        $"Registration email failed for {email}: {emailEx.Message}",
                        emailEx,
                        cancellationToken);
                }

                try
                {
                    await _realtimeNotificationService.NotifyPendingUserCreatedAsync(
                        new PendingUserResponse
                        {
                            IdentityUserId = identityUser.Id,
                            UserId = appUser.Id,
                            UserCode = appUser.UserCode,
                            FullName = fullName,
                            Email = email
                        },
                        cancellationToken);
                }
                catch (Exception notifyEx)
                {
                    await _logger.LogErrorAsync(
                        ApplicationLevelType.Business,
                        LogSourceHelper.Current(),
                        $"Pending user notification failed: {notifyEx.Message}",
                        notifyEx,
                        cancellationToken);
                }

                return ResponseFactory.Success(
                    new LoginResponse
                    {
                        IsSuccess = true,
                        Message = ResponseMessageConstants.RegisterSuccess
                    },
                    ResponseMessageConstants.RegisterSuccess);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    $"Registration error: {ex.Message}",
                    ex,
                    cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var identityUser = await FindIdentityUserByLoginIdAsync(
                    request.LoginId, cancellationToken);

                if (identityUser is null)
                {
                    await _logger.LogWarningAsync(
                        ApplicationLevelType.Business,
                        LogSourceHelper.Current(),
                        $"Login failed for '{request.LoginId}': user not found.",
                        cancellationToken);
                    throw new UnauthorizedException(ValidationMessageConstants.InvalidCredentials);
                }

                if (!await _userManager.CheckPasswordAsync(identityUser, request.Password))
                {
                    await _logger.LogWarningAsync(
                        ApplicationLevelType.Business,
                        LogSourceHelper.Current(),
                        $"Login failed for '{request.LoginId}': invalid password.",
                        cancellationToken);
                    throw new UnauthorizedException(ValidationMessageConstants.InvalidCredentials);
                }

                var appUser = await _context.AppUsers.FirstOrDefaultAsync(
                    x => x.IdentityUserId == identityUser.Id && !x.IsDeleted,
                    cancellationToken);

                if (appUser is null)
                    throw new UnauthorizedException(ValidationMessageConstants.UserProfileNotFound);

                if (!appUser.IsActive)
                    throw new UnauthorizedException("Your account is awaiting administrator approval.");

                var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

                var existingTokens = await _context.RefreshTokens
                    .Where(x => x.IdentityUserId == identityUser.Id && !x.IsRevoked)
                    .ToListAsync(cancellationToken);

                foreach (var token in existingTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedDate = utcNow;
                }

                var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(identityUser, appUser);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                _context.RefreshTokens.Add(new RefreshTokenEntity
                {
                    IdentityUserId = identityUser.Id,
                    Token = refreshToken,
                    ExpiryDate = utcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                    IsRevoked = false,
                    IsActive = true,
                    CreatedDate = utcNow,
                    ModifiedDate = utcNow,
                    CreatedBy = identityUser.Id,
                    ModifiedBy = identityUser.Id
                });

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _currentUserService.SetUnauthenticatedUserContext(
                    appUser.Id, identityUser.Id, identityUser.Email!);

                await _logger.LogInformationAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    $"User {identityUser.UserName} logged in successfully.",
                    cancellationToken);

                return ResponseFactory.Success(
                    new LoginResponse
                    {
                        IsSuccess = true,
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        Expiration = utcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
                    },
                    ResponseMessageConstants.LoginSuccess);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    $"Login error for '{request.LoginId}': {ex.Message}",
                    ex,
                    cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(
            RefreshTokenRequest request,
            CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var identityUserId = _jwtTokenService.GetIdentityUserIdFromExpiredToken(request.AccessToken);
                if (identityUserId is null)
                    throw new UnauthorizedException(ResponseMessageConstants.InvalidToken);

                var stored = await _context.RefreshTokens.FirstOrDefaultAsync(
                    x => x.Token == request.RefreshToken
                         && x.IdentityUserId == identityUserId
                         && !x.IsRevoked,
                    cancellationToken);

                if (stored is null || stored.ExpiryDate < _timeProvider.GetUtcNow().UtcDateTime)
                    throw new UnauthorizedException(ResponseMessageConstants.InvalidRefreshToken);

                var identityUser = await _userManager.FindByIdAsync(identityUserId.Value.ToString());
                if (identityUser is null)
                    throw new UnauthorizedException(ValidationMessageConstants.InvalidCredentials);

                var appUser = await _context.AppUsers.FirstOrDefaultAsync(
                    x => x.IdentityUserId == identityUser.Id && !x.IsDeleted,
                    cancellationToken);

                if (appUser is null || !appUser.IsActive)
                    throw new UnauthorizedException(ValidationMessageConstants.UserAccountIsInactive);

                var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

                stored.IsRevoked = true;
                stored.RevokedDate = utcNow;

                var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(identityUser, appUser);
                var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

                _context.RefreshTokens.Add(new RefreshTokenEntity
                {
                    IdentityUserId = identityUser.Id,
                    Token = newRefreshToken,
                    ExpiryDate = utcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                    IsRevoked = false,
                    IsActive = true,
                    CreatedDate = utcNow,
                    ModifiedDate = utcNow,
                    CreatedBy = identityUser.Id,
                    ModifiedBy = identityUser.Id
                });

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _currentUserService.SetUnauthenticatedUserContext(
                    appUser.Id, identityUser.Id, identityUser.Email!);

                return ResponseFactory.Success(
                    new LoginResponse
                    {
                        AccessToken = accessToken,
                        RefreshToken = newRefreshToken,
                        Expiration = utcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
                    },
                    ResponseMessageConstants.RefreshTokenSuccess);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    ex.Message,
                    ex,
                    cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var tokens = await _context.RefreshTokens
                    .Where(x => x.IdentityUserId == userId && !x.IsRevoked)
                    .ToListAsync(cancellationToken);

                var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                    token.IsActive = false;
                    token.RevokedDate = utcNow;
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return ResponseFactory.Success(true, ResponseMessageConstants.LogoutSuccess);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    ex.Message,
                    ex,
                    cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(
            ForgotPasswordRequest request,
            CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            try
            {
                var identityUser = await _userManager.FindByEmailAsync(request.Email.Trim());
                if (identityUser is null)
                {
                    return ResponseFactory.Success(true, ResponseMessageConstants.ForgotPasswordSuccess);
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);

                try
                {
                    await _emailService.SendPasswordResetAsync(
                        identityUser.Email!,
                        token,
                        identityUser.UserName,
                        cancellationToken);
                }
                catch (Exception emailEx)
                {
                    await _logger.LogErrorAsync(
                        ApplicationLevelType.Business,
                        LogSourceHelper.Current(),
                        $"Password reset email failed: {emailEx.Message}",
                        emailEx,
                        cancellationToken);
                }

                return ResponseFactory.Success(true, ResponseMessageConstants.ForgotPasswordSuccess);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    ex.Message,
                    ex,
                    cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(
            ResetPasswordRequest request,
            CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var identityUser = await _userManager.FindByEmailAsync(request.Email.Trim());
                if (identityUser is null)
                    throw new UnauthorizedException(ResponseMessageConstants.ResetPasswordFailed);

                var token = Uri.UnescapeDataString(request.Token);
                var result = await _userManager.ResetPasswordAsync(identityUser, token, request.Password);

                if (!result.Succeeded)
                    throw new ValidationException(result.Errors.Select(e => e.Description).ToList());

                var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
                var tokens = await _context.RefreshTokens
                    .Where(x => x.IdentityUserId == identityUser.Id && !x.IsRevoked)
                    .ToListAsync(cancellationToken);

                foreach (var rt in tokens)
                {
                    rt.IsRevoked = true;
                    rt.IsActive = false;
                    rt.RevokedDate = utcNow;
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return ResponseFactory.Success(true, ResponseMessageConstants.ResetPasswordSuccess);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    ex.Message,
                    ex,
                    cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(
            Guid identityUserId,
            ChangePasswordRequest request,
            CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);

            try
            {
                var identityUser = await _userManager.FindByIdAsync(identityUserId.ToString());
                if (identityUser is null)
                    throw new UnauthorizedException(ValidationMessageConstants.InvalidCredentials);

                var result = await _userManager.ChangePasswordAsync(
                    identityUser,
                    request.CurrentPassword,
                    request.NewPassword);

                if (!result.Succeeded)
                    throw new UnauthorizedException(ResponseMessageConstants.ChangePasswordFailed);

                return ResponseFactory.Success(true, ResponseMessageConstants.ChangePasswordSuccess);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    ex.Message,
                    ex,
                    cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<bool>> IsEmailAvailableAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return ResponseFactory.Success(false, "Email is required");

            var existing = await _userManager.FindByEmailAsync(email.Trim());
            var available = existing is null;
            return ResponseFactory.Success(
                available,
                available ? "Email is available" : "Email already exists");
        }

        public async Task<ApiResponse<bool>> IsUserNameAvailableAsync(
            string userName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return ResponseFactory.Success(false, "User name is required");

            var existing = await _userManager.FindByNameAsync(userName.Trim());
            var available = existing is null;
            return ResponseFactory.Success(
                available,
                available ? "User name is available" : "User name already exists");
        }

        /// <summary>
        /// Resolves Identity user by email, username, or AppUsers.UserCode (e.g. FP0001).
        /// </summary>
        private async Task<ApplicationUser?> FindIdentityUserByLoginIdAsync(
            string loginId,
            CancellationToken cancellationToken)
        {
            loginId = loginId.Trim();
            if (string.IsNullOrEmpty(loginId))
                return null;

            var byEmail = await _userManager.FindByEmailAsync(loginId);
            if (byEmail is not null)
                return byEmail;

            var byName = await _userManager.FindByNameAsync(loginId);
            if (byName is not null)
                return byName;

            var appUser = await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.UserCode == loginId && !x.IsDeleted,
                    cancellationToken);

            if (appUser is null)
                return null;

            return await _userManager.FindByIdAsync(appUser.IdentityUserId.ToString());
        }
    }
}
using FinancialPortfolio.Business.Abstractions.IJwtToken;
using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Data.Entities.AppUser;
using FinancialPortfolio.Data.Identity;
using FinancialPortfolio.Models.Common.Constants;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FinancialPortfolio.Business.Services.JwtToken
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationLoggerService _logger;

        public JwtTokenService(IOptions<JwtSettings> jwtOptions, UserManager<ApplicationUser> userManager, IApplicationLoggerService logger)
        {
            _jwtSettings = Guard.AgainstNull(jwtOptions.Value, nameof(jwtOptions.Value));
            _userManager = Guard.AgainstNull(userManager, nameof(userManager));
            _logger = Guard.AgainstNull(logger, nameof(logger));
        }

        public async Task<string> GenerateAccessTokenAsync(ApplicationUser identityUser, AppUserEntity appUser)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var roles = await _userManager.GetRolesAsync(identityUser);
            var fullName = CommonHelper.BuildFullName(appUser.FirstName, appUser.LastName);
            var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, identityUser.Id.ToString()),
        new(ClaimTypes.Email, identityUser.Email ?? string.Empty),
        new(ClaimTypes.Name, identityUser.UserName ?? string.Empty),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new(ClaimConstants.UserId, appUser.Id.ToString()),      // Important
        new(ClaimConstants.UserCode, appUser.UserCode),
        new(ClaimConstants.FullName, fullName)
    };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: credentials);

            await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Generated access token for user {identityUser.UserName} with ID {identityUser.Id} and portfolio user ID {appUser.Id}.");
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(randomBytes);
        }

        public Guid? GetIdentityUserIdFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey))
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                var claim = principal.FindFirst(ClaimTypes.NameIdentifier);

                if (claim is null)
                {
                    return null;
                }

                return Guid.Parse(claim.Value);
            }
            catch
            {
                return null;
            }
        }
    }
}

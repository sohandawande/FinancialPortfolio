using FinancialPortfolio.Data.Entities.AppUser;
using FinancialPortfolio.Data.Identity;

namespace FinancialPortfolio.Business.Abstractions.IJwtToken
{
    public interface IJwtTokenService
    {
        Task<string> GenerateAccessTokenAsync(ApplicationUser identityUser, AppUserEntity appUser);
        string GenerateRefreshToken();
        Guid? GetIdentityUserIdFromExpiredToken(string token);
    }
}

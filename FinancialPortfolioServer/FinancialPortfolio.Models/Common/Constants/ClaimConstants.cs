using System.Security.Claims;

namespace FinancialPortfolio.Models.Common.Constants
{
    public static class ClaimConstants
    {
        public const string UserId = "UserId";
        public const string UserCode = "UserCode";
        public const string FullName = "FullName";
        public const string IdentityUserId = ClaimTypes.NameIdentifier;
        public const string Email = ClaimTypes.Email;
        public const string Role = ClaimTypes.Role;
        public const string UserName = ClaimTypes.Name;
    }
}

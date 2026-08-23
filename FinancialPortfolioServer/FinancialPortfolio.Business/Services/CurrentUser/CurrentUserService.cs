using FinancialPortfolio.Models.Abstractions.ICurrentUser;
using FinancialPortfolio.Models.Common.Constants;
using FinancialPortfolio.Models.Common.Utilities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FinancialPortfolio.Business.Services.CurrentUser
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _accessor;
        private long? _overrideUserId;
        private Guid? _overrideIdentityUserId;
        private string? _overrideEmail;

        public CurrentUserService(IHttpContextAccessor accessor)
        {
            _accessor = Guard.AgainstNull(accessor, nameof(accessor));
        }

        private ClaimsPrincipal? User => _accessor.HttpContext?.User;

        public bool IsAuthenticated => _overrideIdentityUserId.HasValue || User?.Identity?.IsAuthenticated == true;

        public long UserId => _overrideUserId ?? (long.TryParse(User?.FindFirst(ClaimConstants.UserId)?.Value, out var id) ? id : 0);

        public string FullName => User?.FindFirst(ClaimConstants.FullName)?.Value ?? UserName;

        public string UserCode => User?.FindFirst(ClaimConstants.UserCode)?.Value ?? string.Empty;

        public Guid IdentityUserId => _overrideIdentityUserId ?? (Guid.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : Guid.Empty);

        public string Email => _overrideEmail ?? (User?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty);

        public string UserName => User?.Identity?.Name ?? Email;

        public IReadOnlyList<string> Roles => User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList() ?? [];

        public bool IsInRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return false;
            return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Explicitly populates user context data for logging and tracking during unauthenticated pipelines.
        /// </summary>
        public void SetUnauthenticatedUserContext(long userId, Guid identityUserId, string email)
        {
            _overrideUserId = userId;
            _overrideIdentityUserId = identityUserId;
            _overrideEmail = email;
        }
    }
}
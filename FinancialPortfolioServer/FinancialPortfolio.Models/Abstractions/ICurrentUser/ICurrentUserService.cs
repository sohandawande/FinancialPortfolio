namespace FinancialPortfolio.Models.Abstractions.ICurrentUser
{
    public interface ICurrentUserService
    {
        long UserId { get; }
        Guid IdentityUserId { get; }
        string Email { get; }
        string UserName { get; }
        string FullName { get; }
        string UserCode { get; }
        bool IsAuthenticated { get; }
        IReadOnlyList<string> Roles { get; }
        bool IsInRole(string role);
        void SetUnauthenticatedUserContext(long userId, Guid identityUserId, string email);
    }
}

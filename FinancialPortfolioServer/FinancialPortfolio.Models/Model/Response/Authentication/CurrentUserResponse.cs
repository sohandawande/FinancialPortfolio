namespace FinancialPortfolio.Models.Model.Response.Authentication
{
    public sealed class CurrentUserResponse
    {
        public Guid IdentityUserId { get; set; }
        public long UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public IReadOnlyList<string> Roles { get; set; } = [];
    }
}

using FinancialPortfolio.Models.Common.Base;

namespace FinancialPortfolio.Models.Model.Response.User
{
    public sealed class PendingUserResponse : BaseModel
    {
        public Guid IdentityUserId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}

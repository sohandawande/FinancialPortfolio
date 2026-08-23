using FinancialPortfolio.Models.Common.Base;

namespace FinancialPortfolio.Models.Model.Response.AppUser
{
    public sealed class UserDetailResponse : BaseModel
    {
        public Guid IdentityUserId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public List<string> Roles { get; set; } = [];
    }
}

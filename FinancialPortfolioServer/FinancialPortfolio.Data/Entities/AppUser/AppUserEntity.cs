using FinancialPortfolio.Data.Identity;

namespace FinancialPortfolio.Data.Entities.AppUser
{
    public class AppUserEntity
    {
        public long Id { get; set; }
        public Guid IdentityUserId { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public virtual ApplicationUser IdentityUser { get; set; } = null!;
    }
}

using FinancialPortfolio.Data.Identity;
namespace FinancialPortfolio.Data.Entities.RefreshToken
{
    public class RefreshTokenEntity
    {
        public long Id { get; set; }
        public Guid IdentityUserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? RevokedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public virtual ApplicationUser IdentityUser { get; set; } = null!;
    }
}

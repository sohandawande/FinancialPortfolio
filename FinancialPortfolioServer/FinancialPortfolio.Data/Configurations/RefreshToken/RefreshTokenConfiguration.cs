namespace FinancialPortfolio.Data.Configurations.RefreshToken
{
    using FinancialPortfolio.Data.Entities.RefreshToken;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Defines the <see cref="RefreshTokenConfiguration" />
    /// </summary>
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
    {
        /// <summary>
        /// The Configure
        /// </summary>
        /// <param name="builder">The builder<see cref="EntityTypeBuilder{RefreshTokenEntity}"/></param>
        public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.Id);
            builder.Property(rt => rt.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(rt => rt.Token)
                   .IsRequired();

            builder.HasOne(rt => rt.IdentityUser)
                   .WithMany()
                   .HasForeignKey(rt => rt.IdentityUserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

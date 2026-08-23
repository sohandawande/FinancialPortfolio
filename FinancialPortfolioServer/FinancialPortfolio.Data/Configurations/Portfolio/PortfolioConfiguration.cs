using FinancialPortfolio.Data.Entities.Portfolio;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPortfolio.Data.Configurations.Portfolio
{
    public class PortfolioConfiguration : IEntityTypeConfiguration<PortfolioEntity>
    {
        public void Configure(EntityTypeBuilder<PortfolioEntity> builder)
        {
            builder.ToTable("Portfolios");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.UserId).IsRequired();

            builder.Property(x => x.Name)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(x => x.Description)
                   .HasMaxLength(500);

            builder.Property(x => x.IsActive)
                   .IsRequired()
                   .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(x => x.UserId).IsUnique();
            builder.HasIndex(x => new { x.UserId, x.IsActive });

            // Audit
            builder.Property(x => x.CreatedBy);
            builder.Property(x => x.ModifiedBy);
            builder.Property(x => x.CreatedDate).IsRequired();
            builder.Property(x => x.ModifiedDate).IsRequired();

            // FK → AppUser (use navigation)
            builder.HasOne(x => x.User)
                   .WithOne()
                   .HasForeignKey<PortfolioEntity>(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Children
            builder.HasMany(x => x.PortfolioStockHolds)
                   .WithOne(x => x.Portfolio)
                   .HasForeignKey(x => x.PortfolioId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.PortfolioStockDividends)
                   .WithOne(x => x.Portfolio)
                   .HasForeignKey(x => x.PortfolioId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

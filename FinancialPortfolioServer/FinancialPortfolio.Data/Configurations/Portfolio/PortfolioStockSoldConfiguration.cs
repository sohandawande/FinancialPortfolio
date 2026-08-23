using FinancialPortfolio.Data.Entities.Portfolio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPortfolio.Data.Configurations.Portfolio
{
    public class PortfolioStockSoldConfiguration : IEntityTypeConfiguration<PortfolioStockSoldEntity>
    {
        public void Configure(EntityTypeBuilder<PortfolioStockSoldEntity> builder)
        {
            builder.ToTable("PortfolioStockSolds");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.PortfolioStockHoldId).IsRequired();

            builder.Property(x => x.SellQuantity).IsRequired();

            builder.Property(x => x.SellPrice)
                   .HasPrecision(18, 4)
                   .IsRequired();

            builder.Property(x => x.HoldDays);

            builder.Property(x => x.LotStatus)
                   .HasConversion<string>()
                   .HasMaxLength(20)          // fixed: was varchar(10)
                   .IsRequired();

            builder.Property(x => x.SoldDate).IsRequired();

            builder.Property(x => x.SoldNotes)
                   .HasMaxLength(1000);

            // Audit
            builder.Property(x => x.CreatedBy);
            builder.Property(x => x.ModifiedBy);
            builder.Property(x => x.CreatedDate).IsRequired();
            builder.Property(x => x.ModifiedDate).IsRequired();

            // Indexes
            builder.HasIndex(x => x.PortfolioStockHoldId);
            builder.HasIndex(x => x.SoldDate);
            builder.HasIndex(x => x.LotStatus);

            // FK → Hold
            builder.HasOne(x => x.PortfolioStockHold)
                   .WithMany(x => x.PortfolioStockSolds)
                   .HasForeignKey(x => x.PortfolioStockHoldId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

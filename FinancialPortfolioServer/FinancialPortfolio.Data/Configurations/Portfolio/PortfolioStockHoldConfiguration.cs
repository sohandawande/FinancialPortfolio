using FinancialPortfolio.Data.Entities.Portfolio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPortfolio.Data.Configurations.Portfolio
{
    public class PortfolioStockHoldConfiguration : IEntityTypeConfiguration<PortfolioStockHoldEntity>
    {
        public void Configure(EntityTypeBuilder<PortfolioStockHoldEntity> builder)
        {
            builder.ToTable("PortfolioStockHolds");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.PortfolioId).IsRequired();
            builder.Property(x => x.StockId).IsRequired();

            builder.Property(x => x.Exchange)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.Quantity).IsRequired();
            builder.Property(x => x.RemainingQuantity).IsRequired();

            builder.Property(x => x.PurchasePrice)
                   .HasPrecision(18, 4)
                   .IsRequired();

            builder.Property(x => x.InvestmentAmount)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(x => x.HoldDays);

            builder.Property(x => x.LotStatus)
                   .HasConversion<string>()
                   .HasMaxLength(20)          // fixed: was varchar(10)
                   .IsRequired();

            builder.Property(x => x.IsSold)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(x => x.PurchaseDate).IsRequired();
            builder.Property(x => x.ExitDate);

            builder.Property(x => x.HoldNotes)
                   .HasMaxLength(1000);

            // Audit
            builder.Property(x => x.CreatedBy);
            builder.Property(x => x.ModifiedBy);
            builder.Property(x => x.CreatedDate).IsRequired();
            builder.Property(x => x.ModifiedDate).IsRequired();

            // Indexes
            builder.HasIndex(x => x.PortfolioId);
            builder.HasIndex(x => x.StockId);
            builder.HasIndex(x => x.LotStatus);
            builder.HasIndex(x => x.IsSold);
            builder.HasIndex(x => new { x.PortfolioId, x.IsSold });
            builder.HasIndex(x => new { x.PortfolioId, x.LotStatus });

            // FK → Portfolio
            builder.HasOne(x => x.Portfolio)
                   .WithMany(x => x.PortfolioStockHolds)
                   .HasForeignKey(x => x.PortfolioId)
                   .OnDelete(DeleteBehavior.Cascade);

            // FK → Stock  (IMPORTANT: use navigation property)
            builder.HasOne(x => x.Stock)
                   .WithMany()
                   .HasForeignKey(x => x.StockId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Children
            builder.HasMany(x => x.PortfolioStockSolds)
                   .WithOne(x => x.PortfolioStockHold)
                   .HasForeignKey(x => x.PortfolioStockHoldId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

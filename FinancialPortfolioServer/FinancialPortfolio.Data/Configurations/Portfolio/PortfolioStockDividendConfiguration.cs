using FinancialPortfolio.Data.Entities.Portfolio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPortfolio.Data.Configurations.Portfolio
{
    public class PortfolioStockDividendConfiguration : IEntityTypeConfiguration<PortfolioStockDividendEntity>
    {
        public void Configure(EntityTypeBuilder<PortfolioStockDividendEntity> builder)
        {
            builder.ToTable("PortfolioStockDividends");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.PortfolioId).IsRequired();
            builder.Property(x => x.StockId).IsRequired();
            builder.Property(x => x.Quantity).IsRequired();

            builder.Property(x => x.PerShareAmount)
                   .HasPrecision(18, 4)
                   .IsRequired();

            builder.Property(x => x.Amount)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(x => x.DividendDate).IsRequired();
            builder.Property(x => x.ExDate);
            builder.Property(x => x.RecordDate);

            builder.Property(x => x.Notes)
                   .HasMaxLength(1000);

            builder.Property(x => x.CreatedBy);
            builder.Property(x => x.ModifiedBy);
            builder.Property(x => x.CreatedDate).IsRequired();
            builder.Property(x => x.ModifiedDate).IsRequired();

            builder.HasIndex(x => x.PortfolioId);
            builder.HasIndex(x => x.StockId);
            builder.HasIndex(x => x.DividendDate);
            builder.HasIndex(x => new { x.PortfolioId, x.StockId });

            builder.HasOne(x => x.Portfolio)
                   .WithMany(x => x.PortfolioStockDividends)
                   .HasForeignKey(x => x.PortfolioId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Stock)
                   .WithMany()
                   .HasForeignKey(x => x.StockId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

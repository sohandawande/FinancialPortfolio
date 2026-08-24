using FinancialPortfolio.Data.Entities.Portfolio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPortfolio.Data.Configurations.Portfolio
{
    public class PortfolioMutualFundConfiguration : IEntityTypeConfiguration<PortfolioMutualFundEntity>
    {
        public void Configure(EntityTypeBuilder<PortfolioMutualFundEntity> builder)
        {
            builder.ToTable("PortfolioMutualFunds");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.PortfolioId).IsRequired();
            builder.Property(x => x.SchemeName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Amc).HasMaxLength(120).IsRequired();
            builder.Property(x => x.FolioNumber).HasMaxLength(60);
            builder.Property(x => x.SchemeCode);
            builder.Property(x => x.NavAsOf);
            builder.Property(x => x.NavSource).HasMaxLength(20);
            builder.Property(x => x.SchemeType).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Units).HasPrecision(18, 4).IsRequired();
            builder.Property(x => x.AverageNav).HasPrecision(18, 4).IsRequired();
            builder.Property(x => x.CurrentNav).HasPrecision(18, 4).IsRequired();
            builder.Property(x => x.InvestedAmount).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.PurchaseDate).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
            builder.HasIndex(x => x.PortfolioId);
            builder.HasOne(x => x.Portfolio)
                .WithMany(x => x.PortfolioMutualFunds)
                .HasForeignKey(x => x.PortfolioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

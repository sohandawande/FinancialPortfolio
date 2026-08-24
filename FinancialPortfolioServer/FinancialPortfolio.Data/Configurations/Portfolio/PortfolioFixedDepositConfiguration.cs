using FinancialPortfolio.Data.Entities.Portfolio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPortfolio.Data.Configurations.Portfolio
{
    public class PortfolioFixedDepositConfiguration : IEntityTypeConfiguration<PortfolioFixedDepositEntity>
    {
        public void Configure(EntityTypeBuilder<PortfolioFixedDepositEntity> builder)
        {
            builder.ToTable("PortfolioFixedDeposits");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.PortfolioId).IsRequired();
            builder.Property(x => x.BankName).HasMaxLength(120).IsRequired();
            builder.Property(x => x.AccountRef).HasMaxLength(60);
            builder.Property(x => x.Principal).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.InterestRate).HasPrecision(8, 4).IsRequired();
            builder.Property(x => x.TenureMonths).IsRequired();
            builder.Property(x => x.InterestType).HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(x => x.StartDate).IsRequired();
            builder.Property(x => x.MaturityDate).IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.HasIndex(x => x.PortfolioId);
            builder.HasOne(x => x.Portfolio)
                .WithMany(x => x.PortfolioFixedDeposits)
                .HasForeignKey(x => x.PortfolioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

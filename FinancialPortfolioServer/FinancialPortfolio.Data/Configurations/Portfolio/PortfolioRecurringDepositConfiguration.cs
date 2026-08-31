using FinancialPortfolio.Data.Entities.Portfolio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPortfolio.Data.Configurations.Portfolio
{
    public class PortfolioRecurringDepositConfiguration : IEntityTypeConfiguration<PortfolioRecurringDepositEntity>
    {
        public void Configure(EntityTypeBuilder<PortfolioRecurringDepositEntity> builder)
        {
            builder.ToTable("PortfolioRecurringDeposits");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.PortfolioId).IsRequired();
            builder.Property(x => x.BankName).HasMaxLength(120).IsRequired();
            builder.Property(x => x.BankIfsc).HasMaxLength(15);
            builder.Property(x => x.AccountRef).HasMaxLength(60);
            builder.Property(x => x.LinkedAccountNumber).HasMaxLength(40);
            builder.Property(x => x.LinkedIfsc).HasMaxLength(15);
            builder.Property(x => x.MonthlyAmount).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.InterestRate).HasPrecision(8, 4).IsRequired();
            builder.Property(x => x.TenureMonths).IsRequired();
            builder.Property(x => x.InstallmentsPaid).IsRequired();
            builder.Property(x => x.StartDate).IsRequired();
            builder.Property(x => x.MaturityDate).IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.HasIndex(x => x.PortfolioId);
            builder.HasOne(x => x.Portfolio)
                .WithMany(x => x.PortfolioRecurringDeposits)
                .HasForeignKey(x => x.PortfolioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

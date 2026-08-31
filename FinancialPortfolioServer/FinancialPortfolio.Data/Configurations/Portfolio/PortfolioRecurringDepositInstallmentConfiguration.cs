using FinancialPortfolio.Data.Entities.Portfolio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPortfolio.Data.Configurations.Portfolio
{
    public class PortfolioRecurringDepositInstallmentConfiguration
        : IEntityTypeConfiguration<PortfolioRecurringDepositInstallmentEntity>
    {
        public void Configure(EntityTypeBuilder<PortfolioRecurringDepositInstallmentEntity> builder)
        {
            builder.ToTable("PortfolioRecurringDepositInstallments");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.RecurringDepositId).IsRequired();
            builder.Property(x => x.InstallmentNumber).IsRequired();
            builder.Property(x => x.DueDate).IsRequired();
            builder.Property(x => x.PaidDate);
            builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.FromBankName).HasMaxLength(120);
            builder.Property(x => x.FromAccountNumber).HasMaxLength(40);
            builder.Property(x => x.FromIfsc).HasMaxLength(15);
            builder.Property(x => x.TransactionReference).HasMaxLength(80);
            builder.Property(x => x.PaymentMode).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.PenaltyAmount).HasPrecision(18, 2);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasIndex(x => x.RecurringDepositId);
            builder.HasIndex(x => new { x.RecurringDepositId, x.InstallmentNumber }).IsUnique();

            builder.HasOne(x => x.RecurringDeposit)
                .WithMany(x => x.Installments)
                .HasForeignKey(x => x.RecurringDepositId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using FinancialPortfolio.Data.Entities.Portfolio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPortfolio.Data.Configurations.Portfolio
{
    public class PortfolioInsurancePolicyConfiguration : IEntityTypeConfiguration<PortfolioInsurancePolicyEntity>
    {
        public void Configure(EntityTypeBuilder<PortfolioInsurancePolicyEntity> builder)
        {
            builder.ToTable("PortfolioInsurancePolicies");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.PortfolioId).IsRequired();
            builder.Property(x => x.InsurerName).HasMaxLength(120).IsRequired();
            builder.Property(x => x.PolicyNumber).HasMaxLength(60).IsRequired();
            builder.Property(x => x.PlanName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.PolicyType).HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(x => x.SumAssured).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.PremiumAmount).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.PremiumFrequency).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.PremiumPayingTermYears).IsRequired();
            builder.Property(x => x.PolicyTermYears).IsRequired();
            builder.Property(x => x.StartDate).IsRequired();
            builder.Property(x => x.MaturityDate).IsRequired();
            builder.Property(x => x.PremiumsPaid).IsRequired();
            builder.Property(x => x.ExpectedMaturityAmount).HasPrecision(18, 2);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.HasIndex(x => x.PortfolioId);
            builder.HasIndex(x => new { x.PortfolioId, x.PolicyNumber }).IsUnique();
            builder.HasOne(x => x.Portfolio)
                .WithMany(x => x.PortfolioInsurancePolicies)
                .HasForeignKey(x => x.PortfolioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

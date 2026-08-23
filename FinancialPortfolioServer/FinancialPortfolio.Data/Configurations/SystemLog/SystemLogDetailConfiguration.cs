using FinancialPortfolio.Data.Entities.SystemLog;
using FinancialPortfolio.QueryEngine.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPortfolio.Data.Configurations.SystemLog
{
    public sealed class SystemLogDetailConfiguration : IEntityTypeConfiguration<SystemLogDetailEntity>
    {
        public void Configure(EntityTypeBuilder<SystemLogDetailEntity> builder)
        {
            builder.ToTable("SystemLogDetails");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(x => x.Exception)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.StackTrace)
                .HasColumnType("nvarchar(max)");

            builder.Property(u => u.CreatedBy)
                   .IsRequired();

            builder.Property(u => u.ModifiedBy)
                   .IsRequired();

            builder.Property(u => u.CreatedDate)
                   .IsRequired()
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(u => u.ModifiedDate)
                   .IsRequired()
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            // 1:1 Relationship Mapping
            builder.HasOne(x => x.SystemLog)
                   .WithOne(y => y.SystemLogDetail)
                   .HasForeignKey<SystemLogDetailEntity>(y => y.SystemLogId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using FinancialPortfolio.Data.Entities.SystemLog;
using FinancialPortfolio.QueryEngine.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPortfolio.Data.Configurations.SystemLog
{
    public sealed class SystemLogConfiguration : IEntityTypeConfiguration<SystemLogEntity>
    {
        public void Configure(EntityTypeBuilder<SystemLogEntity> builder)
        {
            builder.ToTable("SystemLogs");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(x => x.LogLevel)
                .HasColumnType("varchar(20)")
                .HasConversion<string>()
                .IsRequired()
                .HasAnnotation(QueryEngineMetadata.Searchable, true)
                .HasAnnotation(QueryEngineMetadata.Filterable, true)
                .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(x => x.ApplicationLevel)
                .HasColumnType("varchar(100)")
                .HasConversion<string>()
                .IsRequired()
                .HasAnnotation(QueryEngineMetadata.Searchable, true)
                .HasAnnotation(QueryEngineMetadata.Filterable, true)
                .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(x => x.Category)
                .HasMaxLength(100)
                .IsRequired()
                .HasAnnotation(QueryEngineMetadata.Searchable, true)
                .HasAnnotation(QueryEngineMetadata.Filterable, true)
                .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(x => x.Method)
                .HasMaxLength(200)
                .IsRequired()
                .HasAnnotation(QueryEngineMetadata.Searchable, true)
                .HasAnnotation(QueryEngineMetadata.Filterable, true)
                .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(x => x.Message)
                .HasMaxLength(4000)
                .IsRequired()
                .HasAnnotation(QueryEngineMetadata.Searchable, true)
                .HasAnnotation(QueryEngineMetadata.Filterable, true)
                .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(x => x.UserId);

            builder.Property(x => x.RequestPath)
                   .HasMaxLength(500);

            builder.Property(x => x.IpAddress)
                   .HasMaxLength(50);

            builder.Property(x => x.MachineName)
                   .HasMaxLength(100);

            builder.HasIndex(x => x.LogLevel);

            builder.HasIndex(x => x.UserId);

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
        }
    }
}

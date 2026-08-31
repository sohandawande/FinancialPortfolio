namespace FinancialPortfolio.Data.Configurations.Etf
{
    using FinancialPortfolio.Data.Entities.Etf;
    using FinancialPortfolio.QueryEngine.Metadata;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Defines the <see cref="EtfDetailConfiguration" />
    /// </summary>
    public sealed class EtfDetailConfiguration : IEntityTypeConfiguration<EtfDetailEntity>
    {
        /// <summary>
        /// The Configure
        /// </summary>
        /// <param name="builder">The builder<see cref="EntityTypeBuilder{EtfDetailEntity}"/></param>
        public void Configure(EntityTypeBuilder<EtfDetailEntity> builder)
        {
            // Table Mapping
            builder.ToTable("EtfDetails");

            // Primary Key Configuration
            builder.HasKey(sd => sd.Id);
            builder.Property(sd => sd.Id)
                   .ValueGeneratedOnAdd();

            // Properties Constraints
            builder.Property(sd => sd.Category)
                   .HasMaxLength(100)
                   .HasAnnotation(QueryEngineMetadata.Searchable, true)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.CurrentPrice)
                   .IsRequired()
                   .HasPrecision(18, 2)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.PreviousClose)
                   .HasPrecision(18, 2)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.OpenPrice)
                   .HasPrecision(18, 2)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.HighPrice)
                   .HasPrecision(18, 2)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.LowPrice)
                   .HasPrecision(18, 2)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.Volume)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.AverageVolume)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.Week52High)
                   .HasPrecision(18, 5)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.Week52Low)
                   .HasPrecision(18, 5)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.PE)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.EPS)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            // Precision Configuration (Based on your examples like 152362.0677)
            // Total digits: 18, Decimal places: 5
            builder.Property(sd => sd.MarketCap)
                   .HasPrecision(18, 5)
                   .IsRequired()
                   .HasAnnotation(QueryEngineMetadata.Searchable, true)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.PriceChange)
                   .HasPrecision(18, 2)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.IsActive)
                   .HasDefaultValue(true)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(x => x.LogoUrl)
                   .HasMaxLength(500)
                   .IsRequired(false);

            builder.Property(sd => sd.LastUpdated)
                   .IsRequired()
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            // BaseEntity Audit Properties Configurations
            builder.Property(sd => sd.CreatedBy)
                   .IsRequired();

            builder.Property(sd => sd.ModifiedBy)
                   .IsRequired();

            builder.Property(sd => sd.CreatedDate)
                   .IsRequired()
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.ModifiedDate)
                   .IsRequired()
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            // 1:1 Relationship Mapping
            builder.HasOne(sd => sd.Etf)
                   .WithOne(s => s.EtfDetail)
                   .HasForeignKey<EtfDetailEntity>(sd => sd.EtfId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

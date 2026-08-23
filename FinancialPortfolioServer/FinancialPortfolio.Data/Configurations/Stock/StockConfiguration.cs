namespace FinancialPortfolio.Data.Configurations.Stock
{
    using FinancialPortfolio.Data.Entities.Stock;
    using FinancialPortfolio.QueryEngine.Metadata;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Defines the <see cref="StockConfiguration" />
    /// </summary>
    public class StockConfiguration : IEntityTypeConfiguration<StockEntity>
    {
        /// <summary>
        /// The Configure
        /// </summary>
        /// <param name="builder">The builder<see cref="EntityTypeBuilder{StockEntity}"/></param>
        public void Configure(EntityTypeBuilder<StockEntity> builder)
        {
            // Table Mapping
            builder.ToTable("Stocks");

            // Primary Key Configuration
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id)
                   .ValueGeneratedOnAdd();

            // Properties Constraints
            builder.Property(s => s.Symbol)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasAnnotation(QueryEngineMetadata.Searchable, true)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(s => s.CompanyName)
                   .IsRequired()
                   .HasMaxLength(250)
                   .HasAnnotation(QueryEngineMetadata.Searchable, true)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(s => s.Industry)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasAnnotation(QueryEngineMetadata.Searchable, true)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.ISINCode)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasAnnotation(QueryEngineMetadata.Searchable, true)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(sd => sd.Series)
                   .IsRequired()
                   .HasMaxLength(20);

            // Database Indexes
            builder.HasIndex(s => s.Symbol)
                   .IsUnique();

            builder.HasIndex(sd => sd.ISINCode)
                   .IsUnique();

            // BaseEntity Audit Properties Configurations
            builder.Property(s => s.CreatedBy)
                   .IsRequired();

            builder.Property(s => s.ModifiedBy)
                   .IsRequired();

            builder.Property(s => s.CreatedDate)
                   .IsRequired()
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(s => s.ModifiedDate)
                   .IsRequired()
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);
        }
    }
}

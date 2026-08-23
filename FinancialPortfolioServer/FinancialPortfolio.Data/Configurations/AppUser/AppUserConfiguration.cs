namespace FinancialPortfolio.Data.Configurations.User
{
    using FinancialPortfolio.Data.Entities.AppUser;
    using FinancialPortfolio.QueryEngine.Metadata;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Defines the <see cref="AppUserConfiguration" />
    /// </summary>
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUserEntity>
    {
        /// <summary>
        /// The Configure
        /// </summary>
        /// <param name="builder">The builder<see cref="EntityTypeBuilder{AppUserEntity}"/></param>
        public void Configure(EntityTypeBuilder<AppUserEntity> builder)
        {
            builder.ToTable("AppUsers");

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(u => u.UserCode)
                   .HasColumnType("varchar(20)")
                   .IsRequired()
                   .HasAnnotation(QueryEngineMetadata.Searchable, true)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(u => u.FirstName)
                   .HasColumnType("varchar(100)")
                   .IsRequired()
                   .HasAnnotation(QueryEngineMetadata.Searchable, true)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(u => u.LastName)
                   .HasColumnType("varchar(100)")
                   .IsRequired()
                   .HasAnnotation(QueryEngineMetadata.Searchable, true)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(u => u.FullName)
                   .HasColumnType("varchar(200)")
                   .IsRequired()
                   .HasAnnotation(QueryEngineMetadata.Searchable, true)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.Property(u => u.MobileNumber)
                   .HasMaxLength(10)
                   .HasAnnotation(QueryEngineMetadata.Searchable, true)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

            builder.HasIndex(u => u.UserCode)
                   .IsUnique()
                   .HasAnnotation(QueryEngineMetadata.Searchable, true)
                   .HasAnnotation(QueryEngineMetadata.Filterable, true)
                   .HasAnnotation(QueryEngineMetadata.Sortable, true);

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

            builder.HasOne(u => u.IdentityUser)
                   .WithOne()
                   .HasForeignKey<AppUserEntity>(x => x.IdentityUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

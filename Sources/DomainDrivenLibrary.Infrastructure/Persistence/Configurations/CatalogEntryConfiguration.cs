using DomainDrivenLibrary.CatalogEntries;
using DomainDrivenLibrary.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainDrivenLibrary.Persistence.Configurations;

/// <summary>
///     EF Core configuration for the <see cref="CatalogEntry" /> entity.
///     Maps the aggregate to the CatalogEntries table with ISBN as the natural primary key.
/// </summary>
public sealed class CatalogEntryConfiguration : IEntityTypeConfiguration<CatalogEntry>
{
    public void Configure(EntityTypeBuilder<CatalogEntry> builder)
    {
        builder.ToTable("catalog_entries");

        // ISBN is the natural primary key (no surrogate key)
        builder.HasKey(x => x.Isbn);

        builder.Property(x => x.Isbn)
            .HasColumnName("isbn")
            .HasMaxLength(13)
            .HasConversion<IsbnConverter>()
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Author)
            .HasColumnName("author")
            .HasMaxLength(255)
            .IsRequired();
    }
}
using DomainDrivenLibrary.Books;
using DomainDrivenLibrary.Books.Identifier;
using DomainDrivenLibrary.Borrowers;
using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.CatalogEntries;
using DomainDrivenLibrary.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainDrivenLibrary.Persistence.Configurations;

/// <summary>
///     EF Core configuration for the <see cref="Book" /> entity.
///     Maps the aggregate to the Books table with foreign keys to CatalogEntries and Borrowers.
/// </summary>
public sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("books");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasMaxLength(36)
            .HasConversion<BookIdConverter>()
            .IsRequired();

        builder.Property(x => x.Isbn)
            .HasColumnName("isbn")
            .HasMaxLength(13)
            .HasConversion<IsbnConverter>()
            .IsRequired();

        builder.Property(x => x.BorrowerId)
            .HasColumnName("borrowed_by")
            .HasMaxLength(36)
            .HasConversion<BorrowerIdConverter>();

        builder.Property(x => x.BorrowedOn)
            .HasColumnName("borrowed_on");

        // Foreign key to CatalogEntry (ISBN)
        builder.HasOne<CatalogEntry>()
            .WithMany()
            .HasForeignKey(x => x.Isbn)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Foreign key to Borrower (nullable - book may not be borrowed)
        builder.HasOne<Borrower>()
            .WithMany()
            .HasForeignKey(x => x.BorrowerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Indexes for query performance
        builder.HasIndex(x => x.Isbn)
            .HasDatabaseName("ix_books_isbn");

        builder.HasIndex(x => x.BorrowerId)
            .HasDatabaseName("ix_books_borrowed_by");
    }
}

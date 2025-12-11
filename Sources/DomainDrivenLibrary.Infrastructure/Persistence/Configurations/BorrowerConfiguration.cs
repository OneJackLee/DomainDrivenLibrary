using DomainDrivenLibrary.Borrowers;
using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainDrivenLibrary.Persistence.Configurations;

/// <summary>
///     EF Core configuration for the <see cref="Borrower" /> entity.
///     Maps the aggregate to the Borrowers table.
/// </summary>
public sealed class BorrowerConfiguration : IEntityTypeConfiguration<Borrower>
{
    public void Configure(EntityTypeBuilder<Borrower> builder)
    {
        builder.ToTable("borrowers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasMaxLength(36)
            .HasConversion<BorrowerIdConverter>()
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.EmailAddress)
            .HasColumnName("email")
            .HasMaxLength(255)
            .HasConversion<EmailAddressConverter>()
            .IsRequired();
    }
}

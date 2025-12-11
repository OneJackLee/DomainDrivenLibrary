using DomainDrivenLibrary.Books;
using DomainDrivenLibrary.Borrowers;
using DomainDrivenLibrary.CatalogEntries;
using DomainDrivenLibrary.Data;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivenLibrary;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<CatalogEntry> CatalogEntries => Set<CatalogEntry>();

    public DbSet<Book> Books => Set<Book>();

    public DbSet<Borrower> Borrowers => Set<Borrower>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration implementations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

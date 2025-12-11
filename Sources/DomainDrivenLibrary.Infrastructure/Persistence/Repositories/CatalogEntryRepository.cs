using DomainDrivenLibrary.CatalogEntries;
using DomainDrivenLibrary.CatalogEntries.ValueObjects;
using DomainDrivenLibrary.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivenLibrary.Persistence.Repositories;

/// <summary>
///     EF Core implementation of <see cref="ICatalogEntryRepository" />.
/// </summary>
internal sealed class CatalogEntryRepository(AppDbContext dbContext) : ICatalogEntryRepository, IScopedDependency
{
    /// <inheritdoc />
    public async Task<CatalogEntry?> GetByIsbnAsync(Isbn isbn, CancellationToken cancellationToken = default)
    {
        return await dbContext.CatalogEntries
            .FirstOrDefaultAsync(c => c.Isbn == isbn, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CatalogEntry>> GetByIsbnsAsync(
        IEnumerable<Isbn> isbns,
        CancellationToken cancellationToken = default)
    {
        var isbnList = isbns.ToList();

        return await dbContext.CatalogEntries
            .Where(c => isbnList.Contains(c.Isbn))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Isbn isbn, CancellationToken cancellationToken = default)
    {
        return await dbContext.CatalogEntries
            .AnyAsync(c => c.Isbn == isbn, cancellationToken);
    }

    /// <inheritdoc />
    public void Add(CatalogEntry catalogEntry) => dbContext.CatalogEntries.Add(catalogEntry);
}
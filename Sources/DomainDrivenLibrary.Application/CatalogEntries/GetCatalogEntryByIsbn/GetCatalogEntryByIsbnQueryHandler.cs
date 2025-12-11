using DomainDrivenLibrary.CatalogEntries.Shared;
using DomainDrivenLibrary.CatalogEntries.ValueObjects;
using DomainDrivenLibrary.Dependencies;

namespace DomainDrivenLibrary.CatalogEntries.GetCatalogEntryByIsbn;

/// <summary>
///     Handles the query to retrieve a catalog entry by ISBN.
/// </summary>
public sealed class GetCatalogEntryByIsbnQueryHandler(ICatalogEntryRepository catalogEntryRepository)
    : IScopedDependency
{
    /// <summary>
    ///     Executes the query to retrieve a catalog entry by ISBN.
    /// </summary>
    /// <param name="query">The query containing the ISBN.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The catalog entry details.</returns>
    /// <exception cref="ArgumentException">Thrown when ISBN format is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the catalog entry is not found.</exception>
    public async Task<CatalogEntryDetailsDto> HandleAsync(
        GetCatalogEntryByIsbnQuery query,
        CancellationToken cancellationToken = default)
    {
        // Parse and validate ISBN
        var isbn = Isbn.Create(query.Isbn);

        // Load the catalog entry
        var catalogEntry = await catalogEntryRepository.GetByIsbnAsync(isbn, cancellationToken);
        if (catalogEntry is null)
        {
            throw new InvalidOperationException(
                $"Catalog entry with ISBN '{query.Isbn}' was not found.");
        }

        return CatalogEntryDetailsDto.FromDomain(catalogEntry);
    }
}

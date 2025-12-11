using DomainDrivenLibrary.CatalogEntries.ValueObjects;

namespace DomainDrivenLibrary.CatalogEntries;

/// <summary>
///     Repository interface for <see cref="CatalogEntry" /> aggregate persistence.
/// </summary>
public interface ICatalogEntryRepository
{
    /// <summary>
    ///     Gets a catalog entry by its ISBN.
    /// </summary>
    /// <param name="isbn">The ISBN identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The catalog entry if found; otherwise, null.</returns>
    Task<CatalogEntry?> GetByIsbnAsync(Isbn isbn, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets multiple catalog entries by their ISBNs.
    /// </summary>
    /// <param name="isbns">The ISBN identifiers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of matching catalog entries.</returns>
    Task<IReadOnlyList<CatalogEntry>> GetByIsbnsAsync(
        IEnumerable<Isbn> isbns,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks if a catalog entry with the specified ISBN exists.
    /// </summary>
    /// <param name="isbn">The ISBN identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the catalog entry exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(Isbn isbn, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a new catalog entry to the repository.
    /// </summary>
    /// <param name="catalogEntry">The catalog entry to add.</param>
    void Add(CatalogEntry catalogEntry);
}

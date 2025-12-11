namespace DomainDrivenLibrary.CatalogEntries.GetCatalogEntryByIsbn;

/// <summary>
///     Query to retrieve a catalog entry by its ISBN.
/// </summary>
/// <param name="Isbn">The ISBN of the catalog entry to retrieve.</param>
public sealed record GetCatalogEntryByIsbnQuery(string Isbn);

using DomainDrivenLibrary.CatalogEntries;

namespace DomainDrivenLibrary.CatalogEntries.Shared;

/// <summary>
///     Data transfer object representing a catalog entry with full details.
///     This DTO is the application layer's output contract for catalog entry operations.
/// </summary>
/// <param name="Isbn">The ISBN of the catalog entry.</param>
/// <param name="Title">The book title.</param>
/// <param name="Author">The book author.</param>
public sealed record CatalogEntryDetailsDto(string Isbn, string Title, string Author)
{
    /// <summary>
    ///     Maps a domain CatalogEntry to this DTO.
    /// </summary>
    /// <param name="catalogEntry">The catalog entry from the domain.</param>
    /// <returns>A new <see cref="CatalogEntryDetailsDto" /> instance.</returns>
    public static CatalogEntryDetailsDto FromDomain(CatalogEntry catalogEntry)
    {
        return new CatalogEntryDetailsDto(
            catalogEntry.Isbn.Value,
            catalogEntry.Title,
            catalogEntry.Author);
    }
}

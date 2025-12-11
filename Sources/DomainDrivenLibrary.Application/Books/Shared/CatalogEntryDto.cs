using DomainDrivenLibrary.CatalogEntries;

namespace DomainDrivenLibrary.Books.Shared;

/// <summary>
///     Data transfer object representing catalog entry information.
///     Nested within BookDetailsDto to reflect domain structure.
/// </summary>
/// <param name="Title">The book title.</param>
/// <param name="Author">The book author.</param>
public sealed record CatalogEntryDto(string Title, string Author)
{
    /// <summary>
    ///     Maps a domain CatalogEntry to this DTO.
    /// </summary>
    /// <param name="catalogEntry">The catalog entry from the domain.</param>
    /// <returns>A new <see cref="CatalogEntryDto" /> instance.</returns>
    public static CatalogEntryDto FromDomain(CatalogEntry catalogEntry)
    {
        return new CatalogEntryDto(catalogEntry.Title, catalogEntry.Author);
    }
}

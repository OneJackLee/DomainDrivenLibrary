using DomainDrivenLibrary.CatalogEntries.Shared;

namespace DomainDrivenLibrary.Contracts.Responses;

/// <summary>
///     API response representing a catalog entry with full details.
/// </summary>
/// <param name="Isbn">The ISBN of the catalog entry.</param>
/// <param name="Title">The book title.</param>
/// <param name="Author">The book author.</param>
public sealed record CatalogEntryDetailsResponse(string Isbn, string Title, string Author)
{
    /// <summary>
    ///     Maps from an application DTO to API response.
    /// </summary>
    public static CatalogEntryDetailsResponse FromDto(CatalogEntryDetailsDto dto)
    {
        return new CatalogEntryDetailsResponse(dto.Isbn, dto.Title, dto.Author);
    }
}

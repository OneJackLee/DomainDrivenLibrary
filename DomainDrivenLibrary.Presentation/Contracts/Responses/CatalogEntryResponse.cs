using DomainDrivenLibrary.Books.Shared;

namespace DomainDrivenLibrary.Contracts.Responses;

/// <summary>
///     API response representing catalog entry information.
/// </summary>
/// <param name="Title">The book title.</param>
/// <param name="Author">The book author.</param>
public sealed record CatalogEntryResponse(string Title, string Author)
{
    /// <summary>
    ///     Maps from an application DTO to API response.
    /// </summary>
    public static CatalogEntryResponse FromDto(CatalogEntryDto dto)
    {
        return new CatalogEntryResponse(dto.Title, dto.Author);
    }
}

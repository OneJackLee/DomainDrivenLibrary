using DomainDrivenLibrary.Books.GetAllBooks;

namespace DomainDrivenLibrary.Contracts.Responses;

/// <summary>
///     API response representing a book with its catalog details.
/// </summary>
/// <param name="Id">The unique identifier of the book copy.</param>
/// <param name="Isbn">The ISBN of the book.</param>
/// <param name="CatalogEntry">The catalog entry containing title and author.</param>
/// <param name="IsAvailable">Whether the book is available for borrowing.</param>
/// <param name="BorrowedBy">The ID of the borrower if currently borrowed; otherwise, null.</param>
public sealed record BookResponse(
    string Id,
    string Isbn,
    CatalogEntryResponse CatalogEntry,
    bool IsAvailable,
    string? BorrowedBy)
{
    /// <summary>
    ///     Maps from an application DTO to API response.
    /// </summary>
    public static BookResponse FromDto(BookDetailsDto dto)
    {
        return new BookResponse(
            dto.Id,
            dto.Isbn,
            CatalogEntryResponse.FromDto(dto.CatalogEntry),
            dto.IsAvailable,
            dto.BorrowedBy);
    }
}

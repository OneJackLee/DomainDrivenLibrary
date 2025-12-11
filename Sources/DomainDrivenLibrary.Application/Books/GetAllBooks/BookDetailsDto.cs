using DomainDrivenLibrary.Books.Models;
using DomainDrivenLibrary.Books.Shared;
using DomainDrivenLibrary.CatalogEntries;

namespace DomainDrivenLibrary.Books.GetAllBooks;

/// <summary>
///     Data transfer object representing a book with its catalog details.
///     This DTO is the application layer's output contract.
/// </summary>
/// <param name="Id">The unique identifier of the book copy.</param>
/// <param name="Isbn">The ISBN of the book.</param>
/// <param name="CatalogEntry">The catalog entry containing title and author.</param>
/// <param name="IsAvailable">Whether the book is available for borrowing.</param>
/// <param name="BorrowedBy">The ID of the borrower if currently borrowed; otherwise, null.</param>
public sealed record BookDetailsDto(
    string Id,
    string Isbn,
    CatalogEntryDto CatalogEntry,
    bool IsAvailable,
    string? BorrowedBy)
{
    /// <summary>
    ///     Maps a domain read model to this DTO.
    /// </summary>
    /// <param name="source">The book with catalog information from the domain.</param>
    /// <returns>A new <see cref="BookDetailsDto" /> instance.</returns>
    public static BookDetailsDto FromDomain(BookWithCatalog source)
    {
        return new BookDetailsDto(
            source.Book.Id.Value,
            source.Book.Isbn.Value,
            CatalogEntryDto.FromDomain(source.CatalogEntry),
            source.Book.IsAvailable,
            source.Book.BorrowerId?.Value);
    }

    /// <summary>
    ///     Creates a BookDetailsDto from individual domain components.
    /// </summary>
    /// <param name="book">The book aggregate.</param>
    /// <param name="catalogEntry">The catalog entry.</param>
    /// <returns>A new <see cref="BookDetailsDto" /> instance.</returns>
    public static BookDetailsDto FromDomain(Book book, CatalogEntry catalogEntry)
    {
        return new BookDetailsDto(
            book.Id.Value,
            book.Isbn.Value,
            CatalogEntryDto.FromDomain(catalogEntry),
            book.IsAvailable,
            book.BorrowerId?.Value);
    }
}

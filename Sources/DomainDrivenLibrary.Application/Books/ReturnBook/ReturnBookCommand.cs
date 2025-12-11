namespace DomainDrivenLibrary.Books.ReturnBook;

/// <summary>
///     Command to return a borrowed book to the library.
/// </summary>
/// <param name="BookId">The unique identifier of the book to return.</param>
/// <param name="BorrowerId">The unique identifier of the borrower returning the book.</param>
public sealed record ReturnBookCommand(string BookId, string BorrowerId);

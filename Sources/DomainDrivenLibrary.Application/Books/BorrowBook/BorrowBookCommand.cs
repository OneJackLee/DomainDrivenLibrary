namespace DomainDrivenLibrary.Books.BorrowBook;

/// <summary>
///     Command to borrow a book from the library.
/// </summary>
/// <param name="BookId">The unique identifier of the book to borrow.</param>
/// <param name="BorrowerId">The unique identifier of the borrower.</param>
public sealed record BorrowBookCommand(string BookId, string BorrowerId);

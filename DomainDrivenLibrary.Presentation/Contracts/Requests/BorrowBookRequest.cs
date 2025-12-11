namespace DomainDrivenLibrary.Contracts.Requests;

/// <summary>
///     Request to borrow a book.
/// </summary>
/// <param name="BorrowerId">The ID of the borrower checking out the book.</param>
public sealed record BorrowBookRequest(string BorrowerId);

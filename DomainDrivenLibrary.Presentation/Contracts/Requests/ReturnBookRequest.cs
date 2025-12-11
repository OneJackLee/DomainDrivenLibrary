namespace DomainDrivenLibrary.Contracts.Requests;

/// <summary>
///     Request to return a borrowed book.
/// </summary>
/// <param name="BorrowerId">The ID of the borrower returning the book.</param>
public sealed record ReturnBookRequest(string BorrowerId);

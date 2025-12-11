namespace DomainDrivenLibrary.Contracts.Requests;

/// <summary>
///     Request to register a new book copy in the library.
/// </summary>
/// <param name="Isbn">The ISBN of the book (10 or 13 digits).</param>
/// <param name="Title">The title of the book.</param>
/// <param name="Author">The author of the book.</param>
public sealed record RegisterBookRequest(string Isbn, string Title, string Author);

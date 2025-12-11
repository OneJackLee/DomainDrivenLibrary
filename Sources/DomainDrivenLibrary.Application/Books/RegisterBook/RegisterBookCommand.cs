namespace DomainDrivenLibrary.Books.RegisterBook;

/// <summary>
///     Command to register a new book copy in the library.
/// </summary>
/// <param name="Isbn">The ISBN of the book (10 or 13 digits).</param>
/// <param name="Title">The book title (required if new ISBN, validated if existing).</param>
/// <param name="Author">The book author (required if new ISBN, validated if existing).</param>
public sealed record RegisterBookCommand(string Isbn, string Title, string Author);

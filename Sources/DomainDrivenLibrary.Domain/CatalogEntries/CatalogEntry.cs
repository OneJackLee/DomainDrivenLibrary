using DomainDrivenLibrary.CatalogEntries.ValueObjects;

namespace DomainDrivenLibrary.CatalogEntries;

/// <summary>
///     Represents a catalog entry in the library system.
///     A catalog entry is the abstract concept of a book identified by its ISBN,
///     containing the book metadata (title and author).
/// </summary>
public sealed class CatalogEntry
{
    private CatalogEntry()
    {
        // Reserved for ORM.
    }

    private CatalogEntry(
        Isbn isbn,
        string title,
        string author)
    {
        Isbn = isbn;
        Title = title;
        Author = author;
    }

    /// <summary>
    ///     The ISBN (International Standard Book Number) that uniquely identifies this catalog entry.
    ///     This serves as the natural key/identity for the aggregate.
    ///     Once set, cannot be chanaged.
    /// </summary>
    public Isbn Isbn { get; private init; } = null!;

    /// <summary>
    ///     The author(s) of the book.
    /// </summary>
    public string Author { get; private set; } = null!;

    /// <summary>
    ///     The title of the book.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    ///     Creates a new catalog entry with the specified ISBN, title, and author.
    /// </summary>
    /// <param name="isbn">The ISBN value object identifying this book.</param>
    /// <param name="title">The book title (cannot be empty).</param>
    /// <param name="author">The book author (cannot be empty).</param>
    /// <returns>A new <see cref="CatalogEntry" /> instance.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="title" /> or <paramref name="author" /> is null or whitespace.
    /// </exception>
    public static CatalogEntry Create(
        Isbn isbn,
        string title,
        string author)
    {
        // Validate title - books must have a title
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title must not be empty.", nameof(title));
        }

        // Validate author - books must have an author
        if (string.IsNullOrWhiteSpace(author))
        {
            throw new ArgumentException("Author must not be empty.", nameof(author));
        }

        return new CatalogEntry(isbn, title, author);
    }

    /// <summary>
    ///     Creates a new catalog entry by parsing the ISBN string.
    /// </summary>
    /// <param name="isbn">The ISBN string to parse (supports ISBN-10 and ISBN-13 formats).</param>
    /// <param name="title">The book title (cannot be empty).</param>
    /// <param name="author">The book author (cannot be empty).</param>
    /// <returns>A new <see cref="CatalogEntry" /> instance.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="isbn" /> is invalid, or when
    ///     <paramref name="title" /> or <paramref name="author" /> is null or whitespace.
    /// </exception>
    public static CatalogEntry Create(
        string isbn,
        string title,
        string author)
    {
        // Attempt to parse the ISBN string into a value object
        if (Isbn.TryParse(isbn, out Isbn? result) && result is not null)
        {
            return Create(result, title, author);
        }

        throw new ArgumentException(
            $"ISBN provided is not valid. Current provided value is '{isbn}'.",
            nameof(isbn));
    }

    /// <summary>
    ///     Updates the author of this catalog entry.
    /// </summary>
    /// <param name="author">The new author value.</param>
    /// <returns>This <see cref="CatalogEntry" /> instance for fluent chaining.</returns>
    /// <remarks>
    ///     The update is only applied if the new value differs from the current value.
    /// </remarks>
    public CatalogEntry UpdateAuthor(string author)
    {
        if (Author != author)
            Author = author;

        return this;
    }

    /// <summary>
    ///     Updates the title of this catalog entry.
    /// </summary>
    /// <param name="title">The new title value.</param>
    /// <returns>This <see cref="CatalogEntry" /> instance for fluent chaining.</returns>
    /// <remarks>
    ///     The update is only applied if the new value differs from the current value.
    /// </remarks>
    public CatalogEntry UpdateTitle(string title)
    {
        if (Title != title)
            Title = title;

        return this;
    }
}
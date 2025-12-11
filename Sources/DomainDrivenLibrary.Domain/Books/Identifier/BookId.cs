namespace DomainDrivenLibrary.Books.Identifier;

/// <summary>
///     Strongly-typed identifier for a book.
///     Prevents accidental mixing of book IDs with other ID types.
/// </summary>
public sealed record BookId
{
    private BookId(string value)
    {
        Value = value;
    }

    /// <summary>
    ///     The underlying string value of the identifier.
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Create a book Id.
    /// </summary>
    /// <param name="value">The Book Id in string.</param>
    /// <returns>BookId identifier.</returns>
    public static BookId Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        value = value.ToUpperInvariant();
        return new BookId(value);
    }

    public static implicit operator string(BookId id)
    {
        return id.Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
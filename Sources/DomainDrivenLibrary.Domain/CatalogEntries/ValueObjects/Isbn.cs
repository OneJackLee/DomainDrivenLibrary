namespace DomainDrivenLibrary.CatalogEntries.ValueObjects;

/// <summary>
///     Represents an ISBN (International Standard Book Number).
///     Supports both ISBN-10 and ISBN-13 formats.
/// </summary>
public sealed record Isbn
{
    private Isbn(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        string normalized = Normalize(value);

        if (!IsValidFormat(normalized))
        {
            throw new ArgumentException(
                $"ISBN must be 10 or 13 digits. Provided value '{value}' has {normalized.Length} digits.",
                nameof(value));
        }

        Value = normalized;
    }
    /// <summary>
    ///     The normalized ISBN value (digits only, no hyphens or spaces).
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Create an ISBN.
    /// </summary>
    /// <param name="value">The ISBN string to parse.</param>
    /// <returns>ISBN value object.</returns>
    public static Isbn Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new Isbn(Normalize(value));
    }

    /// <summary>
    ///     Attempts to parse a string into an ISBN without throwing an exception.
    /// </summary>
    /// <param name="value">The ISBN string to parse.</param>
    /// <param name="result">The parsed ISBN if successful; otherwise, null.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    public static bool TryParse(string? value, out Isbn? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        string normalized = Normalize(value);

        if (!IsValidFormat(normalized))
            return false;

        result = new Isbn(normalized);
        return true;
    }

    /// <summary>
    ///     Normalizes the ISBN by removing hyphens, spaces, and converting to uppercase.
    /// </summary>
    private static string Normalize(string value)
    {
        return value
            .Replace("-", "")
            .Replace(" ", "")
            .ToUpperInvariant();
    }

    /// <summary>
    ///     Validates that the normalized ISBN has the correct length (10 or 13 digits).
    ///     ISBN-10 may end with 'X' as a check digit.
    /// </summary>
    private static bool IsValidFormat(string normalized)
    {
        if (normalized.Length == 10)
        {
            // ISBN-10: 9 digits + 1 check character (digit or 'X')
            for (int i = 0; i < 9; i++)
            {
                if (!char.IsDigit(normalized[i]))
                    return false;
            }
            return char.IsDigit(normalized[9]) || normalized[9] == 'X';
        }

        if (normalized.Length != 13)
            return false;

        // ISBN-13: all digits
        return normalized.All(char.IsDigit);
    }

    public static implicit operator string(Isbn isbn)
    {
        return isbn.Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
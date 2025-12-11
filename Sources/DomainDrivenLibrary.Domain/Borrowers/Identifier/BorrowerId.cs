namespace DomainDrivenLibrary.Borrowers.Identifier;

/// <summary>
///     Strongly-typed identifier for a borrower.
///     Prevents accidental mixing of borrower IDs with other ID types.
/// </summary>
public sealed record BorrowerId
{
    private BorrowerId(string value)
    {
        Value = value;
    }

    /// <summary>
    ///     The underlying string value of the identifier.
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Create a borrowserId.
    /// </summary>
    /// <param name="value">The borrower Id in string.</param>
    /// <returns>BorrowerId identifier.</returns>
    public static BorrowerId Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        value = value.ToUpperInvariant();
        return new BorrowerId(value);
    }

    public static implicit operator string(BorrowerId id)
    {
        return id.Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
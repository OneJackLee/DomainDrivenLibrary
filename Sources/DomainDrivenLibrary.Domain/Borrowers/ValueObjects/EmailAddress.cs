using System.Net.Mail;

namespace DomainDrivenLibrary.Borrowers.ValueObjects;

/// <summary>
///     Represents a validated email address.
/// </summary>
public sealed record EmailAddress
{
    private EmailAddress(string value)
    {
        Value = value;
    }

    /// <summary>
    ///     The normalized email address (lowercase).
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Creates a new EmailAddress with validation.
    /// </summary>
    /// <param name="value">The email address string.</param>
    /// <returns>A validated <see cref="EmailAddress" /> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when value is null, whitespace, or invalid format.</exception>
    public static EmailAddress Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!TryParseInternal(value, out string? normalized))
        {
            throw new ArgumentException(
                $"Invalid email format: '{value}'.",
                nameof(value));
        }

        return new EmailAddress(normalized!);
    }

    /// <summary>
    ///     Attempts to parse a string into an EmailAddress without throwing an exception.
    /// </summary>
    /// <param name="value">The email address string to parse.</param>
    /// <param name="result">The parsed EmailAddress if successful; otherwise, null.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    public static bool TryParse(string? value, out EmailAddress? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!TryParseInternal(value, out string? normalized))
            return false;

        result = new EmailAddress(normalized!);
        return true;
    }

    /// <summary>
    ///     Validates email and normalizes to lowercase.
    /// </summary>
    private static bool TryParseInternal(string value, out string? normalized)
    {
        normalized = null;

        if (!MailAddress.TryCreate(value, out MailAddress? mailAddress))
            return false;

        normalized = mailAddress.Address.ToLowerInvariant();
        return true;
    }

    public static implicit operator string(EmailAddress email)
    {
        return email.Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
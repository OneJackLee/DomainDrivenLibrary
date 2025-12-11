using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.Borrowers.ValueObjects;
using DomainDrivenLibrary.Entities;

namespace DomainDrivenLibrary.Borrowers;

/// <summary>
///     Represents a library member who can borrow books.
/// </summary>
public sealed class Borrower : EntityBase<BorrowerId>
{
    private Borrower()
    {
        // Reserved for ORM.
    }

    private Borrower(
        BorrowerId id,
        string name,
        EmailAddress emailAddress) : base(id)
    {
        Id = id;
        Name = name;
        EmailAddress = emailAddress;
    }

    /// <summary>
    ///     The borrower's name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    ///     The borrower's email address.
    /// </summary>
    public EmailAddress EmailAddress { get; private set; } = null!;

    /// <summary>
    ///     Register a new borrower.
    /// </summary>
    /// <param name="id">The unique identifier for the borrower.</param>
    /// <param name="name">The borrower's name (cannot be empty).</param>
    /// <param name="emailAddress">The borrower's validated email address.</param>
    /// <returns>A new <see cref="Borrower" /> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name" /> is null or whitespace.</exception>
    public static Borrower Register(
        BorrowerId id,
        string name,
        EmailAddress emailAddress)
    {
        // Validate name - borrowers must have a name for identification
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must not be empty.", nameof(name));
        }

        return new Borrower(id, name, emailAddress);
    }

    /// <summary>
    ///     Updates the borrower's name.
    /// </summary>
    /// <param name="name">The new name value.</param>
    /// <returns>This <see cref="Borrower" /> instance for fluent chaining.</returns>
    /// <remarks>
    ///     The update is only applied if the new value differs from the current value.
    /// </remarks>
    public Borrower UpdateName(string name)
    {
        if (Name != name)
            Name = name;

        return this;
    }

    /// <summary>
    ///     Updates the borrower's email address.
    /// </summary>
    /// <param name="emailAddress">The new email address.</param>
    /// <returns>This <see cref="Borrower" /> instance for fluent chaining.</returns>
    /// <remarks>
    ///     The update is only applied if the new value differs from the current value.
    /// </remarks>
    public Borrower UpdateEmailAddress(EmailAddress emailAddress)
    {
        if (EmailAddress != emailAddress)
            EmailAddress = emailAddress;

        return this;
    }
}
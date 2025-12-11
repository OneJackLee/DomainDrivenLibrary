namespace DomainDrivenLibrary.Borrowers.RegisterBorrower;

/// <summary>
///     Command to register a new borrower in the library system.
/// </summary>
/// <param name="Name">The borrower's name (required, non-empty).</param>
/// <param name="Email">The borrower's email address (required, valid format).</param>
public sealed record RegisterBorrowerCommand(string Name, string Email);

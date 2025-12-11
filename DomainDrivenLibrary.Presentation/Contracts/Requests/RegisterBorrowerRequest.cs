namespace DomainDrivenLibrary.Contracts.Requests;

/// <summary>
///     Request to register a new borrower in the library.
/// </summary>
/// <param name="Name">The name of the borrower.</param>
/// <param name="Email">The email address of the borrower.</param>
public sealed record RegisterBorrowerRequest(string Name, string Email);

namespace DomainDrivenLibrary.Contracts.Responses;

/// <summary>
///     Standard API error response.
/// </summary>
/// <param name="Error">The error type/category.</param>
/// <param name="Message">The error message.</param>
public sealed record ErrorResponse(string Error, string Message);

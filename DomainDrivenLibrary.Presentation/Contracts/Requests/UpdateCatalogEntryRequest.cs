namespace DomainDrivenLibrary.Contracts.Requests;

/// <summary>
///     Request to update an existing catalog entry.
/// </summary>
/// <param name="Title">The new title for the catalog entry.</param>
/// <param name="Author">The new author for the catalog entry.</param>
public sealed record UpdateCatalogEntryRequest(string Title, string Author);

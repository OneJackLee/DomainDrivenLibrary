namespace DomainDrivenLibrary.CatalogEntries.UpdateCatalogEntry;

/// <summary>
///     Command to update the title and author of a catalog entry.
/// </summary>
/// <param name="Isbn">The ISBN of the catalog entry to update.</param>
/// <param name="Title">The new title for the catalog entry.</param>
/// <param name="Author">The new author for the catalog entry.</param>
public sealed record UpdateCatalogEntryCommand(string Isbn, string Title, string Author);

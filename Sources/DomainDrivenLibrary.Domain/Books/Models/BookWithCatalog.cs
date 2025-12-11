using DomainDrivenLibrary.CatalogEntries;

namespace DomainDrivenLibrary.Books.Models;

/// <summary>
///     Read model combining a book with its catalog entry information.
///     DO NOT RETURN THIS TO PRESENTATION LAYER. THIS MUST BE TRANSFORM INTO A DTO.
/// </summary>
/// <param name="Book">The book entity.</param>
/// <param name="CatalogEntry">The associated catalog entry.</param>
public sealed record BookWithCatalog(Book Book, CatalogEntry CatalogEntry);
using DomainDrivenLibrary.CatalogEntries.Shared;
using DomainDrivenLibrary.CatalogEntries.ValueObjects;
using DomainDrivenLibrary.Data;
using DomainDrivenLibrary.Dependencies;

namespace DomainDrivenLibrary.CatalogEntries.UpdateCatalogEntry;

/// <summary>
///     Handles the update of a catalog entry's title and author.
/// </summary>
public sealed class UpdateCatalogEntryCommandHandler(
    ICatalogEntryRepository catalogEntryRepository,
    IUnitOfWork unitOfWork)
    : IScopedDependency
{
    /// <summary>
    ///     Executes the command to update a catalog entry.
    /// </summary>
    /// <param name="command">The update command containing the new title and author.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated catalog entry details.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when ISBN format is invalid, or title/author is empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the catalog entry is not found.
    /// </exception>
    public async Task<CatalogEntryDetailsDto> HandleAsync(
        UpdateCatalogEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate title
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            throw new ArgumentException("Title must not be empty.", nameof(command.Title));
        }

        // Validate author
        if (string.IsNullOrWhiteSpace(command.Author))
        {
            throw new ArgumentException("Author must not be empty.", nameof(command.Author));
        }

        // Parse and validate ISBN
        var isbn = Isbn.Create(command.Isbn);

        // Load the catalog entry
        var catalogEntry = await catalogEntryRepository.GetByIsbnAsync(isbn, cancellationToken);
        if (catalogEntry is null)
        {
            throw new InvalidOperationException(
                $"Catalog entry with ISBN '{command.Isbn}' was not found.");
        }

        // Update the catalog entry
        catalogEntry
            .UpdateTitle(command.Title)
            .UpdateAuthor(command.Author);

        // Persist
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CatalogEntryDetailsDto.FromDomain(catalogEntry);
    }
}

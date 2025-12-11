using DomainDrivenLibrary.Books.GetAllBooks;
using DomainDrivenLibrary.Books.Identifier;
using DomainDrivenLibrary.CatalogEntries;
using DomainDrivenLibrary.CatalogEntries.ValueObjects;
using DomainDrivenLibrary.Data;
using DomainDrivenLibrary.Dependencies;
using DomainDrivenLibrary.Identifier;

namespace DomainDrivenLibrary.Books.RegisterBook;

/// <summary>
///     Handles the registration of a new book copy in the library.
/// </summary>
public sealed class RegisterBookCommandHandler(
    IBookRepository bookRepository,
    ICatalogEntryRepository catalogEntryRepository,
    IIdGenerator idGenerator,
    IUnitOfWork unitOfWork)
    : IScopedDependency
{
    /// <summary>
    ///     Executes the command to register a new book copy.
    /// </summary>
    /// <param name="command">The registration command containing book details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly registered book details.</returns>
    /// <exception cref="ArgumentException">Thrown when ISBN format is invalid.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when ISBN exists with different title/author (conflict).
    /// </exception>
    public async Task<BookDetailsDto> HandleAsync(
        RegisterBookCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate and create ISBN value object
        var isbn = Isbn.Create(command.Isbn);

        // Check if catalog entry already exists
        var existingCatalogEntry = await catalogEntryRepository.GetByIsbnAsync(isbn, cancellationToken);
        CatalogEntry catalogEntry;

        if (existingCatalogEntry is not null)
        {
            // ISBN exists - verify title and author match
            ValidateCatalogEntryMatch(existingCatalogEntry, command.Title, command.Author);
            catalogEntry = existingCatalogEntry;
        }
        else
        {
            // ISBN doesn't exist - create new catalog entry
            catalogEntry = CatalogEntry.Create(isbn, command.Title, command.Author);
            catalogEntryRepository.Add(catalogEntry);
        }

        // Generate new BookId and create the book
        var bookId = BookId.Create(idGenerator.New());
        var book = Book.Register(bookId, isbn);

        // Persist
        bookRepository.Add(book);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BookDetailsDto.FromDomain(book, catalogEntry);
    }

    /// <summary>
    ///     Validates that the provided title and author match the existing catalog entry.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when there's a mismatch.</exception>
    private static void ValidateCatalogEntryMatch(
        CatalogEntry existingEntry,
        string providedTitle,
        string providedAuthor)
    {
        var titleMatches = string.Equals(
            existingEntry.Title,
            providedTitle,
            StringComparison.OrdinalIgnoreCase);

        var authorMatches = string.Equals(
            existingEntry.Author,
            providedAuthor,
            StringComparison.OrdinalIgnoreCase);

        if (!titleMatches || !authorMatches)
        {
            throw new InvalidOperationException(
                $"ISBN '{existingEntry.Isbn}' already exists with different metadata. " +
                $"Expected: Title='{existingEntry.Title}', Author='{existingEntry.Author}'. " +
                $"Provided: Title='{providedTitle}', Author='{providedAuthor}'.");
        }
    }
}
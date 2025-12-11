using DomainDrivenLibrary.Books.Identifier;
using DomainDrivenLibrary.Books.Models;

namespace DomainDrivenLibrary.Books;

/// <summary>
///     Repository interface for <see cref="Book" /> aggregate persistence.
/// </summary>
public interface IBookRepository
{
    /// <summary>
    ///     Gets a book by its unique identifier.
    /// </summary>
    /// <param name="id">The book identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The book if found; otherwise, null.</returns>
    Task<Book?> GetByIdAsync(BookId id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all books in the library.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of all books.</returns>
    Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all books with their associated catalog entries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of book and catalog entry pairs.</returns>
    Task<IReadOnlyList<BookWithCatalog>> GetAllWithCatalogAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a new book to the repository.
    /// </summary>
    /// <param name="book">The book to add.</param>
    void Add(Book book);
}
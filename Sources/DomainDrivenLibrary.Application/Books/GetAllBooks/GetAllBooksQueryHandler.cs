using DomainDrivenLibrary.Dependencies;

namespace DomainDrivenLibrary.Books.GetAllBooks;

/// <summary>
///     Handles the query to retrieve all books with their catalog information.
/// </summary>
public sealed class GetAllBooksQueryHandler(IBookRepository bookRepository) : IScopedDependency
{

    /// <summary>
    ///     Executes the query to retrieve all books.
    /// </summary>
    /// <param name="query">The query (no parameters).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of book details.</returns>
    public async Task<IReadOnlyList<BookDetailsDto>> HandleAsync(
        GetAllBooksQuery query,
        CancellationToken cancellationToken = default)
    {
        var booksWithCatalog = await bookRepository.GetAllWithCatalogAsync(cancellationToken);

        return booksWithCatalog
            .Select(BookDetailsDto.FromDomain)
            .ToList()
            .AsReadOnly();
    }
}
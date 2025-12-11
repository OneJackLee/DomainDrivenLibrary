using DomainDrivenLibrary.Books;
using DomainDrivenLibrary.Books.Identifier;
using DomainDrivenLibrary.Books.Models;
using DomainDrivenLibrary.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivenLibrary.Persistence.Repositories;

/// <summary>
///     EF Core implementation of <see cref="IBookRepository" />.
/// </summary>
internal sealed class BookRepository(AppDbContext dbContext) : IBookRepository, IScopedDependency
{
    /// <inheritdoc />
    public async Task<Book?> GetByIdAsync(BookId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Books
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Books
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BookWithCatalog>> GetAllWithCatalogAsync(
        CancellationToken cancellationToken = default)
    {
        var books = await dbContext.Books.ToListAsync(cancellationToken);

        if (books.Count == 0)
            return [];

        var isbns = books.Select(b => b.Isbn).Distinct().ToList();

        var catalogEntries = await dbContext.CatalogEntries
            .Where(c => isbns.Contains(c.Isbn))
            .ToDictionaryAsync(c => c.Isbn, cancellationToken);

        return books
            .Select(book => new BookWithCatalog(book, catalogEntries[book.Isbn]))
            .OrderBy(b => b.CatalogEntry.Isbn)
            .ThenByDescending(b => b.Book.IsAvailable)
            .ToList();
    }

    /// <inheritdoc />
    public void Add(Book book) => dbContext.Books.Add(book);
}
using DomainDrivenLibrary.Books.GetAllBooks;
using DomainDrivenLibrary.Books.Identifier;
using DomainDrivenLibrary.Borrowers;
using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.CatalogEntries;
using DomainDrivenLibrary.Data;
using DomainDrivenLibrary.Dependencies;

namespace DomainDrivenLibrary.Books.BorrowBook;

/// <summary>
///     Handles the borrowing of a book by a library member.
/// </summary>
public sealed class BorrowBookCommandHandler(
    IBookRepository bookRepository,
    IBorrowerRepository borrowerRepository,
    ICatalogEntryRepository catalogEntryRepository,
    IUnitOfWork unitOfWork)
    : IScopedDependency
{
    /// <summary>
    ///     Executes the command to borrow a book.
    /// </summary>
    /// <param name="command">The borrow command containing book and borrower IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated book details after borrowing.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the book is not found, borrower is not found, or book is already borrowed.
    /// </exception>
    public async Task<BookDetailsDto> HandleAsync(
        BorrowBookCommand command,
        CancellationToken cancellationToken = default)
    {
        var bookId = BookId.Create(command.BookId);
        var borrowerId = BorrowerId.Create(command.BorrowerId);

        // Load the book
        var book = await bookRepository.GetByIdAsync(bookId, cancellationToken);
        if (book is null)
        {
            throw new InvalidOperationException($"Book with ID '{command.BookId}' was not found.");
        }

        // Verify borrower exists
        var borrowerExists = await borrowerRepository.ExistsAsync(borrowerId, cancellationToken);
        if (!borrowerExists)
        {
            throw new InvalidOperationException($"Borrower with ID '{command.BorrowerId}' was not found.");
        }

        // Borrow the book (domain validates availability)
        book.Borrow(borrowerId);

        // Persist
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch catalog entry to build response
        var catalogEntry = await catalogEntryRepository.GetByIsbnAsync(book.Isbn, cancellationToken);

        return BookDetailsDto.FromDomain(book, catalogEntry!);
    }
}
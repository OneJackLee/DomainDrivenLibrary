using DomainDrivenLibrary.Books.GetAllBooks;
using DomainDrivenLibrary.Books.Identifier;
using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.CatalogEntries;
using DomainDrivenLibrary.Data;
using DomainDrivenLibrary.Dependencies;

namespace DomainDrivenLibrary.Books.ReturnBook;

/// <summary>
///     Handles the return of a borrowed book to the library.
/// </summary>
public sealed class ReturnBookCommandHandler(
    IBookRepository bookRepository,
    ICatalogEntryRepository catalogEntryRepository,
    IUnitOfWork unitOfWork)
    : IScopedDependency
{
    /// <summary>
    ///     Executes the command to return a book.
    /// </summary>
    /// <param name="command">The return command containing book and borrower IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated book details after returning.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the book is not found, book is not borrowed, or borrower doesn't match.
    /// </exception>
    public async Task<BookDetailsDto> HandleAsync(
        ReturnBookCommand command,
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

        // Verify the borrower matches (safety check)
        if (book.BorrowerId is null)
        {
            throw new InvalidOperationException(
                $"Book with ID '{command.BookId}' is not currently borrowed.");
        }

        if (book.BorrowerId != borrowerId)
        {
            throw new InvalidOperationException(
                $"Book with ID '{command.BookId}' was not borrowed by borrower '{command.BorrowerId}'.");
        }

        // Return the book (domain validates it's borrowed)
        book.Return();

        // Persist
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch catalog entry to build response
        var catalogEntry = await catalogEntryRepository.GetByIsbnAsync(book.Isbn, cancellationToken);

        return BookDetailsDto.FromDomain(book, catalogEntry!);
    }
}
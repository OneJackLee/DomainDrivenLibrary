using DomainDrivenLibrary.Books.GetAllBooks;
using DomainDrivenLibrary.Books.Identifier;
using DomainDrivenLibrary.Books.ReturnBook;
using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.CatalogEntries;
using DomainDrivenLibrary.CatalogEntries.ValueObjects;
using DomainDrivenLibrary.Data;
using FluentAssertions;
using NSubstitute;

namespace DomainDrivenLibrary.Books;

public class ReturnBookCommandHandlerTests
{
    #region Test Data

    private const string ValidBookId = "book-123";
    private const string ValidBorrowerId = "borrower-456";
    private const string ValidIsbn = "9780132350884";
    private const string ValidTitle = "Clean Code";
    private const string ValidAuthor = "Robert C. Martin";

    private static Book CreateAvailableBook() =>
        Book.Register(BookId.Create(ValidBookId), Isbn.Create(ValidIsbn));

    private static Book CreateBorrowedBook(string borrowerId = ValidBorrowerId)
    {
        var book = Book.Register(BookId.Create(ValidBookId), Isbn.Create(ValidIsbn));
        book.Borrow(BorrowerId.Create(borrowerId));
        return book;
    }

    private static CatalogEntry CreateCatalogEntry() =>
        CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);

    #endregion

    #region Setup

    private readonly IBookRepository _bookRepository;
    private readonly ICatalogEntryRepository _catalogEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ReturnBookCommandHandler _handler;

    public ReturnBookCommandHandlerTests()
    {
        _bookRepository = Substitute.For<IBookRepository>();
        _catalogEntryRepository = Substitute.For<ICatalogEntryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        // Default setup: book exists and is borrowed by the valid borrower
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns(CreateBorrowedBook());
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(CreateCatalogEntry());

        _handler = new ReturnBookCommandHandler(
            _bookRepository,
            _catalogEntryRepository,
            _unitOfWork);
    }

    #endregion

    #region Success Scenarios

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsBookDetailsDto()
    {
        // Arrange
        var command = new ReturnBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        BookDetailsDto result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(ValidBookId.ToUpperInvariant());
        result.Isbn.Should().Be(ValidIsbn);
        result.CatalogEntry.Title.Should().Be(ValidTitle);
        result.CatalogEntry.Author.Should().Be(ValidAuthor);
        result.IsAvailable.Should().BeTrue();
        result.BorrowedBy.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_SavesChanges()
    {
        // Arrange
        var command = new ReturnBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsTheBook()
    {
        // Arrange
        var book = CreateBorrowedBook();
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns(book);

        var command = new ReturnBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        book.IsAvailable.Should().BeTrue();
        book.BorrowerId.Should().BeNull();
        book.BorrowedOn.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_FetchesCatalogEntryForResponse()
    {
        // Arrange
        var command = new ReturnBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _catalogEntryRepository.Received(1)
            .GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Book Not Found

    [Fact]
    public async Task HandleAsync_WhenBookNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns((Book?)null);

        var command = new ReturnBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*Book*{ValidBookId}*not found*");
    }

    [Fact]
    public async Task HandleAsync_WhenBookNotFound_DoesNotSaveChanges()
    {
        // Arrange
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns((Book?)null);

        var command = new ReturnBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        try { await _handler.HandleAsync(command); } catch { }

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Book Not Borrowed

    [Fact]
    public async Task HandleAsync_WhenBookNotBorrowed_ThrowsInvalidOperationException()
    {
        // Arrange
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns(CreateAvailableBook());

        var command = new ReturnBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not currently borrowed*");
    }

    [Fact]
    public async Task HandleAsync_WhenBookNotBorrowed_DoesNotSaveChanges()
    {
        // Arrange
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns(CreateAvailableBook());

        var command = new ReturnBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        try { await _handler.HandleAsync(command); } catch { }

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Borrower Mismatch

    [Fact]
    public async Task HandleAsync_WhenBorrowerMismatch_ThrowsInvalidOperationException()
    {
        // Arrange - Book is borrowed by a different borrower
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns(CreateBorrowedBook("different-borrower"));

        var command = new ReturnBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*not borrowed by borrower*{ValidBorrowerId}*");
    }

    [Fact]
    public async Task HandleAsync_WhenBorrowerMismatch_DoesNotSaveChanges()
    {
        // Arrange
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns(CreateBorrowedBook("different-borrower"));

        var command = new ReturnBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        try { await _handler.HandleAsync(command); } catch { }

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBorrowerMismatch_DoesNotReturnBook()
    {
        // Arrange
        var book = CreateBorrowedBook("different-borrower");
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns(book);

        var command = new ReturnBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        try { await _handler.HandleAsync(command); } catch { }

        // Assert - Book should still be borrowed
        book.IsAvailable.Should().BeFalse();
        book.BorrowerId.Should().NotBeNull();
    }

    #endregion

    #region Validation Order

    [Fact]
    public async Task HandleAsync_ChecksBookExistsBeforeCheckingBorrowed()
    {
        // Arrange
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns((Book?)null);

        var command = new ReturnBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert - Should throw book not found, not "not borrowed"
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Book*not found*");
    }

    [Fact]
    public async Task HandleAsync_ChecksBookBorrowedBeforeCheckingBorrowerMatch()
    {
        // Arrange - Book exists but is not borrowed
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns(CreateAvailableBook());

        var command = new ReturnBookCommand(ValidBookId, "wrong-borrower");

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert - Should throw "not currently borrowed", not "borrower mismatch"
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not currently borrowed*");
    }

    #endregion

    #region Cancellation Token

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToAllRepositories()
    {
        // Arrange
        var command = new ReturnBookCommand(ValidBookId, ValidBorrowerId);
        using var cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        // Act
        await _handler.HandleAsync(command, token);

        // Assert
        await _bookRepository.Received(1).GetByIdAsync(Arg.Any<BookId>(), token);
        await _catalogEntryRepository.Received(1).GetByIsbnAsync(Arg.Any<Isbn>(), token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }

    #endregion
}

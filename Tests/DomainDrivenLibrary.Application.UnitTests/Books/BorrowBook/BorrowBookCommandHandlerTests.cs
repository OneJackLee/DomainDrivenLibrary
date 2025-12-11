using DomainDrivenLibrary.Books.Identifier;
using DomainDrivenLibrary.Borrowers;
using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.CatalogEntries;
using DomainDrivenLibrary.CatalogEntries.ValueObjects;
using DomainDrivenLibrary.Data;
using FluentAssertions;
using NSubstitute;

namespace DomainDrivenLibrary.Books.BorrowBook;

public class BorrowBookCommandHandlerTests
{

    #region Validation Order

    [Fact]
    public async Task HandleAsync_ChecksBookExistsBeforeBorrowerExists()
    {
        // Arrange
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns((Book?)null);
        _borrowerRepository.ExistsAsync(Arg.Any<BorrowerId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new BorrowBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert - Should throw book not found, not borrower not found
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Book*not found*");
    }

    #endregion

    #region Cancellation Token

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToAllRepositories()
    {
        // Arrange
        var command = new BorrowBookCommand(ValidBookId, ValidBorrowerId);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.HandleAsync(command, token);

        // Assert
        await _bookRepository.Received(1).GetByIdAsync(Arg.Any<BookId>(), token);
        await _borrowerRepository.Received(1).ExistsAsync(Arg.Any<BorrowerId>(), token);
        await _catalogEntryRepository.Received(1).GetByIsbnAsync(Arg.Any<Isbn>(), token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }

    #endregion
    #region Test Data

    private const string ValidBookId = "book-123";
    private const string ValidBorrowerId = "borrower-456";
    private const string ValidIsbn = "9780132350884";
    private const string ValidTitle = "Clean Code";
    private const string ValidAuthor = "Robert C. Martin";

    private static Book CreateAvailableBook() =>
        Book.Register(BookId.Create(ValidBookId), Isbn.Create(ValidIsbn));

    private static Book CreateBorrowedBook(string borrowerId = "existing-borrower")
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
    private readonly IBorrowerRepository _borrowerRepository;
    private readonly ICatalogEntryRepository _catalogEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly BorrowBookCommandHandler _handler;

    public BorrowBookCommandHandlerTests()
    {
        _bookRepository = Substitute.For<IBookRepository>();
        _borrowerRepository = Substitute.For<IBorrowerRepository>();
        _catalogEntryRepository = Substitute.For<ICatalogEntryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        // Default setup: book exists and is available, borrower exists
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns(CreateAvailableBook());
        _borrowerRepository.ExistsAsync(Arg.Any<BorrowerId>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(CreateCatalogEntry());

        _handler = new BorrowBookCommandHandler(
            _bookRepository,
            _borrowerRepository,
            _catalogEntryRepository,
            _unitOfWork);
    }

    #endregion

    #region Success Scenarios

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsBookDetailsDto()
    {
        // Arrange
        var command = new BorrowBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(ValidBookId.ToUpperInvariant());
        result.Isbn.Should().Be(ValidIsbn);
        result.CatalogEntry.Title.Should().Be(ValidTitle);
        result.CatalogEntry.Author.Should().Be(ValidAuthor);
        result.IsAvailable.Should().BeFalse();
        result.BorrowedBy.Should().Be(ValidBorrowerId.ToUpperInvariant());
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_SavesChanges()
    {
        // Arrange
        var command = new BorrowBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_BorrowsTheBook()
    {
        // Arrange
        var book = CreateAvailableBook();
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns(book);

        var command = new BorrowBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        book.IsAvailable.Should().BeFalse();
        book.BorrowerId.Should().NotBeNull();
        book.BorrowerId!.Value.Should().Be(ValidBorrowerId.ToUpperInvariant());
    }

    [Fact]
    public async Task HandleAsync_FetchesCatalogEntryForResponse()
    {
        // Arrange
        var command = new BorrowBookCommand(ValidBookId, ValidBorrowerId);

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

        var command = new BorrowBookCommand(ValidBookId, ValidBorrowerId);

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

        var command = new BorrowBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        try { await _handler.HandleAsync(command); }
        catch {}

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Borrower Not Found

    [Fact]
    public async Task HandleAsync_WhenBorrowerNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _borrowerRepository.ExistsAsync(Arg.Any<BorrowerId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new BorrowBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*Borrower*{ValidBorrowerId}*not found*");
    }

    [Fact]
    public async Task HandleAsync_WhenBorrowerNotFound_DoesNotSaveChanges()
    {
        // Arrange
        _borrowerRepository.ExistsAsync(Arg.Any<BorrowerId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new BorrowBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        try { await _handler.HandleAsync(command); }
        catch {}

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Book Already Borrowed

    [Fact]
    public async Task HandleAsync_WhenBookAlreadyBorrowed_ThrowsInvalidOperationException()
    {
        // Arrange
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns(CreateBorrowedBook());

        var command = new BorrowBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already borrowed*");
    }

    [Fact]
    public async Task HandleAsync_WhenBookAlreadyBorrowed_DoesNotSaveChanges()
    {
        // Arrange
        _bookRepository.GetByIdAsync(Arg.Any<BookId>(), Arg.Any<CancellationToken>())
            .Returns(CreateBorrowedBook());

        var command = new BorrowBookCommand(ValidBookId, ValidBorrowerId);

        // Act
        try { await _handler.HandleAsync(command); }
        catch {}

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}
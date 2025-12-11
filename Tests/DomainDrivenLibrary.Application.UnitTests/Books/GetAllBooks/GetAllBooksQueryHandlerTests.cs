using DomainDrivenLibrary.Books.GetAllBooks;
using DomainDrivenLibrary.Books.Identifier;
using DomainDrivenLibrary.Books.Models;
using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.CatalogEntries;
using DomainDrivenLibrary.CatalogEntries.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DomainDrivenLibrary.Books;

public class GetAllBooksQueryHandlerTests
{
    #region Test Data

    private static Book CreateAvailableBook(string id, string isbn) =>
        Book.Register(BookId.Create(id), Isbn.Create(isbn));

    private static Book CreateBorrowedBook(string id, string isbn, string borrowerId)
    {
        var book = Book.Register(BookId.Create(id), Isbn.Create(isbn));
        book.Borrow(BorrowerId.Create(borrowerId));
        return book;
    }

    private static CatalogEntry CreateCatalogEntry(string isbn, string title, string author) =>
        CatalogEntry.Create(isbn, title, author);

    #endregion

    #region Setup

    private readonly IBookRepository _bookRepository;
    private readonly GetAllBooksQueryHandler _handler;

    public GetAllBooksQueryHandlerTests()
    {
        _bookRepository = Substitute.For<IBookRepository>();
        _bookRepository.GetAllWithCatalogAsync(Arg.Any<CancellationToken>())
            .Returns(new List<BookWithCatalog>());

        _handler = new GetAllBooksQueryHandler(_bookRepository);
    }

    #endregion

    #region Empty Results

    [Fact]
    public async Task HandleAsync_WhenNoBooks_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllBooksQuery();

        // Act
        IReadOnlyList<BookDetailsDto> result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region Single Book

    [Fact]
    public async Task HandleAsync_WithSingleAvailableBook_ReturnsCorrectDto()
    {
        // Arrange
        var book = CreateAvailableBook("book-123", "9780132350884");
        var catalogEntry = CreateCatalogEntry("9780132350884", "Clean Code", "Robert C. Martin");
        var bookWithCatalog = new BookWithCatalog(book, catalogEntry);

        _bookRepository.GetAllWithCatalogAsync(Arg.Any<CancellationToken>())
            .Returns(new List<BookWithCatalog> { bookWithCatalog });

        var query = new GetAllBooksQuery();

        // Act
        IReadOnlyList<BookDetailsDto> result = await _handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(1);
        BookDetailsDto dto = result[0];
        dto.Id.Should().Be("BOOK-123");
        dto.Isbn.Should().Be("9780132350884");
        dto.CatalogEntry.Title.Should().Be("Clean Code");
        dto.CatalogEntry.Author.Should().Be("Robert C. Martin");
        dto.IsAvailable.Should().BeTrue();
        dto.BorrowedBy.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithSingleBorrowedBook_ReturnsCorrectDto()
    {
        // Arrange
        var book = CreateBorrowedBook("book-456", "9780132350884", "borrower-789");
        var catalogEntry = CreateCatalogEntry("9780132350884", "Clean Code", "Robert C. Martin");
        var bookWithCatalog = new BookWithCatalog(book, catalogEntry);

        _bookRepository.GetAllWithCatalogAsync(Arg.Any<CancellationToken>())
            .Returns(new List<BookWithCatalog> { bookWithCatalog });

        var query = new GetAllBooksQuery();

        // Act
        IReadOnlyList<BookDetailsDto> result = await _handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(1);
        BookDetailsDto dto = result[0];
        dto.IsAvailable.Should().BeFalse();
        dto.BorrowedBy.Should().Be("BORROWER-789");
    }

    #endregion

    #region Multiple Books

    [Fact]
    public async Task HandleAsync_WithMultipleBooks_ReturnsAllBooks()
    {
        // Arrange
        var book1 = CreateAvailableBook("book-1", "9780132350884");
        var book2 = CreateAvailableBook("book-2", "9780201633610");
        var book3 = CreateBorrowedBook("book-3", "9780132350884", "borrower-1");

        var catalogEntry1 = CreateCatalogEntry("9780132350884", "Clean Code", "Robert C. Martin");
        var catalogEntry2 = CreateCatalogEntry("9780201633610", "Design Patterns", "Gang of Four");

        _bookRepository.GetAllWithCatalogAsync(Arg.Any<CancellationToken>())
            .Returns(new List<BookWithCatalog>
            {
                new(book1, catalogEntry1),
                new(book2, catalogEntry2),
                new(book3, catalogEntry1)
            });

        var query = new GetAllBooksQuery();

        // Act
        IReadOnlyList<BookDetailsDto> result = await _handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleCopiesOfSameIsbn_ReturnsAllCopies()
    {
        // Arrange - Two copies of the same book
        var book1 = CreateAvailableBook("copy-1", "9780132350884");
        var book2 = CreateBorrowedBook("copy-2", "9780132350884", "borrower-1");
        var catalogEntry = CreateCatalogEntry("9780132350884", "Clean Code", "Robert C. Martin");

        _bookRepository.GetAllWithCatalogAsync(Arg.Any<CancellationToken>())
            .Returns(new List<BookWithCatalog>
            {
                new(book1, catalogEntry),
                new(book2, catalogEntry)
            });

        var query = new GetAllBooksQuery();

        // Act
        IReadOnlyList<BookDetailsDto> result = await _handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(dto => dto.Id == "COPY-1" && dto.IsAvailable);
        result.Should().Contain(dto => dto.Id == "COPY-2" && !dto.IsAvailable);
        result.All(dto => dto.Isbn == "9780132350884").Should().BeTrue();
    }

    #endregion

    #region DTO Mapping

    [Fact]
    public async Task HandleAsync_MapsNestedCatalogEntryCorrectly()
    {
        // Arrange
        var book = CreateAvailableBook("book-1", "9780132350884");
        var catalogEntry = CreateCatalogEntry("9780132350884", "Clean Code", "Robert C. Martin");
        var bookWithCatalog = new BookWithCatalog(book, catalogEntry);

        _bookRepository.GetAllWithCatalogAsync(Arg.Any<CancellationToken>())
            .Returns(new List<BookWithCatalog> { bookWithCatalog });

        var query = new GetAllBooksQuery();

        // Act
        IReadOnlyList<BookDetailsDto> result = await _handler.HandleAsync(query);

        // Assert
        result[0].CatalogEntry.Should().NotBeNull();
        result[0].CatalogEntry.Title.Should().Be("Clean Code");
        result[0].CatalogEntry.Author.Should().Be("Robert C. Martin");
    }

    #endregion

    #region Repository Interaction

    [Fact]
    public async Task HandleAsync_CallsRepository()
    {
        // Arrange
        var query = new GetAllBooksQuery();

        // Act
        await _handler.HandleAsync(query);

        // Assert
        await _bookRepository.Received(1).GetAllWithCatalogAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationToken()
    {
        // Arrange
        var query = new GetAllBooksQuery();
        using var cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        // Act
        await _handler.HandleAsync(query, token);

        // Assert
        await _bookRepository.Received(1).GetAllWithCatalogAsync(token);
    }

    #endregion

    #region Return Type

    [Fact]
    public async Task HandleAsync_ReturnsReadOnlyList()
    {
        // Arrange
        var query = new GetAllBooksQuery();

        // Act
        IReadOnlyList<BookDetailsDto> result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeAssignableTo<IReadOnlyList<BookDetailsDto>>();
    }

    #endregion
}

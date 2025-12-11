using DomainDrivenLibrary.Books.GetAllBooks;
using DomainDrivenLibrary.Books.RegisterBook;
using DomainDrivenLibrary.CatalogEntries;
using DomainDrivenLibrary.CatalogEntries.ValueObjects;
using DomainDrivenLibrary.Data;
using DomainDrivenLibrary.Identifier;
using FluentAssertions;
using NSubstitute;

namespace DomainDrivenLibrary.Books;

public class RegisterBookCommandHandlerTests
{
    #region Test Data

    private const string ValidIsbn = "9780132350884";
    private const string ValidTitle = "Clean Code";
    private const string ValidAuthor = "Robert C. Martin";
    private const string GeneratedId = "01HGXYZ123456";

    #endregion

    #region Setup

    private readonly IBookRepository _bookRepository;
    private readonly ICatalogEntryRepository _catalogEntryRepository;
    private readonly IIdGenerator _idGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RegisterBookCommandHandler _handler;

    public RegisterBookCommandHandlerTests()
    {
        _bookRepository = Substitute.For<IBookRepository>();
        _catalogEntryRepository = Substitute.For<ICatalogEntryRepository>();
        _idGenerator = Substitute.For<IIdGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _idGenerator.New().Returns(GeneratedId);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns((CatalogEntry?)null);

        _handler = new RegisterBookCommandHandler(
            _bookRepository,
            _catalogEntryRepository,
            _idGenerator,
            _unitOfWork);
    }

    #endregion

    #region Success Scenarios - New ISBN

    [Fact]
    public async Task HandleAsync_WithNewIsbn_ReturnsBookDetailsDto()
    {
        // Arrange
        var command = new RegisterBookCommand(ValidIsbn, ValidTitle, ValidAuthor);

        // Act
        BookDetailsDto result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(GeneratedId.ToUpperInvariant());
        result.Isbn.Should().Be(ValidIsbn);
        result.CatalogEntry.Title.Should().Be(ValidTitle);
        result.CatalogEntry.Author.Should().Be(ValidAuthor);
        result.IsAvailable.Should().BeTrue();
        result.BorrowedBy.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithNewIsbn_CreatesCatalogEntry()
    {
        // Arrange
        var command = new RegisterBookCommand(ValidIsbn, ValidTitle, ValidAuthor);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _catalogEntryRepository.Received(1).Add(Arg.Is<CatalogEntry>(ce =>
            ce.Isbn.Value == ValidIsbn &&
            ce.Title == ValidTitle &&
            ce.Author == ValidAuthor));
    }

    [Fact]
    public async Task HandleAsync_WithNewIsbn_AddsBookToRepository()
    {
        // Arrange
        var command = new RegisterBookCommand(ValidIsbn, ValidTitle, ValidAuthor);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _bookRepository.Received(1).Add(Arg.Is<Book>(b =>
            b.Isbn.Value == ValidIsbn &&
            b.IsAvailable));
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_SavesChanges()
    {
        // Arrange
        var command = new RegisterBookCommand(ValidIsbn, ValidTitle, ValidAuthor);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_GeneratesNewId()
    {
        // Arrange
        var command = new RegisterBookCommand(ValidIsbn, ValidTitle, ValidAuthor);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _idGenerator.Received(1).New();
    }

    #endregion

    #region Success Scenarios - Existing ISBN

    [Fact]
    public async Task HandleAsync_WithExistingIsbnAndMatchingMetadata_ReturnsBookDetailsDto()
    {
        // Arrange
        var existingCatalogEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(existingCatalogEntry);

        var command = new RegisterBookCommand(ValidIsbn, ValidTitle, ValidAuthor);

        // Act
        BookDetailsDto result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Isbn.Should().Be(ValidIsbn);
        result.CatalogEntry.Title.Should().Be(ValidTitle);
    }

    [Fact]
    public async Task HandleAsync_WithExistingIsbnAndMatchingMetadata_DoesNotCreateNewCatalogEntry()
    {
        // Arrange
        var existingCatalogEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(existingCatalogEntry);

        var command = new RegisterBookCommand(ValidIsbn, ValidTitle, ValidAuthor);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _catalogEntryRepository.DidNotReceive().Add(Arg.Any<CatalogEntry>());
    }

    [Fact]
    public async Task HandleAsync_WithExistingIsbnAndMatchingMetadata_StillAddsBook()
    {
        // Arrange
        var existingCatalogEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(existingCatalogEntry);

        var command = new RegisterBookCommand(ValidIsbn, ValidTitle, ValidAuthor);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _bookRepository.Received(1).Add(Arg.Any<Book>());
    }

    [Fact]
    public async Task HandleAsync_WithExistingIsbnAndCaseInsensitiveMatch_Succeeds()
    {
        // Arrange
        var existingCatalogEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(existingCatalogEntry);

        // Different case for title and author
        var command = new RegisterBookCommand(ValidIsbn, "CLEAN CODE", "ROBERT C. MARTIN");

        // Act
        BookDetailsDto result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region ISBN Conflict Scenarios

    [Fact]
    public async Task HandleAsync_WithExistingIsbnAndDifferentTitle_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingCatalogEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(existingCatalogEntry);

        var command = new RegisterBookCommand(ValidIsbn, "Different Title", ValidAuthor);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists with different metadata*");
    }

    [Fact]
    public async Task HandleAsync_WithExistingIsbnAndDifferentAuthor_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingCatalogEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(existingCatalogEntry);

        var command = new RegisterBookCommand(ValidIsbn, ValidTitle, "Different Author");

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists with different metadata*");
    }

    [Fact]
    public async Task HandleAsync_WhenIsbnConflict_DoesNotAddBook()
    {
        // Arrange
        var existingCatalogEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(existingCatalogEntry);

        var command = new RegisterBookCommand(ValidIsbn, "Different Title", ValidAuthor);

        // Act
        try { await _handler.HandleAsync(command); } catch { }

        // Assert
        _bookRepository.DidNotReceive().Add(Arg.Any<Book>());
    }

    [Fact]
    public async Task HandleAsync_WhenIsbnConflict_DoesNotSaveChanges()
    {
        // Arrange
        var existingCatalogEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(existingCatalogEntry);

        var command = new RegisterBookCommand(ValidIsbn, "Different Title", ValidAuthor);

        // Act
        try { await _handler.HandleAsync(command); } catch { }

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Invalid Input Scenarios

    [Fact]
    public async Task HandleAsync_WithInvalidIsbn_ThrowsArgumentException()
    {
        // Arrange
        var command = new RegisterBookCommand("invalid-isbn", ValidTitle, ValidAuthor);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*ISBN*");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var command = new RegisterBookCommand(ValidIsbn, "", ValidAuthor);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Title*");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyAuthor_ThrowsArgumentException()
    {
        // Arrange
        var command = new RegisterBookCommand(ValidIsbn, ValidTitle, "");

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Author*");
    }

    [Theory]
    [InlineData("123")] // Too short
    [InlineData("12345678901234")] // Too long (14 digits)
    [InlineData("abcdefghij")] // Non-numeric
    public async Task HandleAsync_WithInvalidIsbnFormat_ThrowsArgumentException(string invalidIsbn)
    {
        // Arrange
        var command = new RegisterBookCommand(invalidIsbn, ValidTitle, ValidAuthor);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region ISBN Normalization

    [Fact]
    public async Task HandleAsync_WithIsbnContainingHyphens_NormalizesIsbn()
    {
        // Arrange
        var command = new RegisterBookCommand("978-0-13-235088-4", ValidTitle, ValidAuthor);

        // Act
        BookDetailsDto result = await _handler.HandleAsync(command);

        // Assert
        result.Isbn.Should().Be("9780132350884");
    }

    [Fact]
    public async Task HandleAsync_WithIsbnContainingSpaces_NormalizesIsbn()
    {
        // Arrange
        var command = new RegisterBookCommand("978 0 13 235088 4", ValidTitle, ValidAuthor);

        // Act
        BookDetailsDto result = await _handler.HandleAsync(command);

        // Assert
        result.Isbn.Should().Be("9780132350884");
    }

    #endregion

    #region Cancellation Token

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToRepositories()
    {
        // Arrange
        var command = new RegisterBookCommand(ValidIsbn, ValidTitle, ValidAuthor);
        using var cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        // Act
        await _handler.HandleAsync(command, token);

        // Assert
        await _catalogEntryRepository.Received(1)
            .GetByIsbnAsync(Arg.Any<Isbn>(), token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }

    #endregion
}

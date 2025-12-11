using DomainDrivenLibrary.CatalogEntries.ValueObjects;
using DomainDrivenLibrary.Data;
using FluentAssertions;
using NSubstitute;

namespace DomainDrivenLibrary.CatalogEntries.UpdateCatalogEntry;

public class UpdateCatalogEntryCommandHandlerTests
{
    #region Test Data

    private const string ValidIsbn = "9780132350884";
    private const string ValidTitle = "Clean Code";
    private const string ValidAuthor = "Robert C. Martin";
    private const string NewTitle = "Clean Code: A Handbook";
    private const string NewAuthor = "Uncle Bob";

    #endregion

    #region Setup

    private readonly ICatalogEntryRepository _catalogEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateCatalogEntryCommandHandler _handler;

    public UpdateCatalogEntryCommandHandlerTests()
    {
        _catalogEntryRepository = Substitute.For<ICatalogEntryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new UpdateCatalogEntryCommandHandler(
            _catalogEntryRepository,
            _unitOfWork);
    }

    #endregion

    #region Success Scenarios

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsCatalogEntryDetailsDto()
    {
        // Arrange
        var existingEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(existingEntry);

        var command = new UpdateCatalogEntryCommand(ValidIsbn, NewTitle, NewAuthor);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Isbn.Should().Be(ValidIsbn);
        result.Title.Should().Be(NewTitle);
        result.Author.Should().Be(NewAuthor);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_UpdatesCatalogEntry()
    {
        // Arrange
        var existingEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(existingEntry);

        var command = new UpdateCatalogEntryCommand(ValidIsbn, NewTitle, NewAuthor);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        existingEntry.Title.Should().Be(NewTitle);
        existingEntry.Author.Should().Be(NewAuthor);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_SavesChanges()
    {
        // Arrange
        var existingEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(existingEntry);

        var command = new UpdateCatalogEntryCommand(ValidIsbn, NewTitle, NewAuthor);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithSameValues_StillSucceeds()
    {
        // Arrange
        var existingEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(existingEntry);

        var command = new UpdateCatalogEntryCommand(ValidIsbn, ValidTitle, ValidAuthor);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Title.Should().Be(ValidTitle);
        result.Author.Should().Be(ValidAuthor);
    }

    #endregion

    #region Not Found Scenarios

    [Fact]
    public async Task HandleAsync_WhenCatalogEntryNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns((CatalogEntry?)null);

        var command = new UpdateCatalogEntryCommand(ValidIsbn, NewTitle, NewAuthor);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task HandleAsync_WhenCatalogEntryNotFound_DoesNotSaveChanges()
    {
        // Arrange
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns((CatalogEntry?)null);

        var command = new UpdateCatalogEntryCommand(ValidIsbn, NewTitle, NewAuthor);

        // Act
        try { await _handler.HandleAsync(command); }
        catch { }

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Validation Scenarios

    [Fact]
    public async Task HandleAsync_WithInvalidIsbn_ThrowsArgumentException()
    {
        // Arrange
        var command = new UpdateCatalogEntryCommand("invalid-isbn", NewTitle, NewAuthor);

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
        var command = new UpdateCatalogEntryCommand(ValidIsbn, "", NewAuthor);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Title*");
    }

    [Fact]
    public async Task HandleAsync_WithWhitespaceTitle_ThrowsArgumentException()
    {
        // Arrange
        var command = new UpdateCatalogEntryCommand(ValidIsbn, "   ", NewAuthor);

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
        var command = new UpdateCatalogEntryCommand(ValidIsbn, NewTitle, "");

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Author*");
    }

    [Fact]
    public async Task HandleAsync_WithWhitespaceAuthor_ThrowsArgumentException()
    {
        // Arrange
        var command = new UpdateCatalogEntryCommand(ValidIsbn, NewTitle, "   ");

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Author*");
    }

    #endregion

    #region Cancellation Token

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToRepository()
    {
        // Arrange
        var existingEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(existingEntry);

        var command = new UpdateCatalogEntryCommand(ValidIsbn, NewTitle, NewAuthor);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.HandleAsync(command, token);

        // Assert
        await _catalogEntryRepository.Received(1).GetByIsbnAsync(Arg.Any<Isbn>(), token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }

    #endregion

    #region ISBN Normalization

    [Fact]
    public async Task HandleAsync_WithIsbnContainingHyphens_NormalizesIsbn()
    {
        // Arrange
        var existingEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(existingEntry);

        var command = new UpdateCatalogEntryCommand("978-0-13-235088-4", NewTitle, NewAuthor);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Isbn.Should().Be("9780132350884");
    }

    #endregion
}

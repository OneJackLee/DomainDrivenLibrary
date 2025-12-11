using DomainDrivenLibrary.CatalogEntries.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DomainDrivenLibrary.CatalogEntries.GetCatalogEntryByIsbn;

public class GetCatalogEntryByIsbnQueryHandlerTests
{
    #region Test Data

    private const string ValidIsbn = "9780132350884";
    private const string ValidTitle = "Clean Code";
    private const string ValidAuthor = "Robert C. Martin";

    #endregion

    #region Setup

    private readonly ICatalogEntryRepository _catalogEntryRepository;
    private readonly GetCatalogEntryByIsbnQueryHandler _handler;

    public GetCatalogEntryByIsbnQueryHandlerTests()
    {
        _catalogEntryRepository = Substitute.For<ICatalogEntryRepository>();

        _handler = new GetCatalogEntryByIsbnQueryHandler(_catalogEntryRepository);
    }

    #endregion

    #region Success Scenarios

    [Fact]
    public async Task HandleAsync_WhenCatalogEntryExists_ReturnsCatalogEntryDetailsDto()
    {
        // Arrange
        var catalogEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(catalogEntry);

        var query = new GetCatalogEntryByIsbnQuery(ValidIsbn);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Isbn.Should().Be(ValidIsbn);
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

        var query = new GetCatalogEntryByIsbnQuery(ValidIsbn);

        // Act
        Func<Task> act = () => _handler.HandleAsync(query);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region Validation Scenarios

    [Fact]
    public async Task HandleAsync_WithInvalidIsbn_ThrowsArgumentException()
    {
        // Arrange
        var query = new GetCatalogEntryByIsbnQuery("invalid-isbn");

        // Act
        Func<Task> act = () => _handler.HandleAsync(query);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*ISBN*");
    }

    [Theory]
    [InlineData("123")]            // Too short
    [InlineData("12345678901234")] // Too long (14 digits)
    [InlineData("abcdefghij")]     // Non-numeric
    public async Task HandleAsync_WithInvalidIsbnFormat_ThrowsArgumentException(string invalidIsbn)
    {
        // Arrange
        var query = new GetCatalogEntryByIsbnQuery(invalidIsbn);

        // Act
        Func<Task> act = () => _handler.HandleAsync(query);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region Repository Interaction

    [Fact]
    public async Task HandleAsync_CallsRepository()
    {
        // Arrange
        var catalogEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(catalogEntry);

        var query = new GetCatalogEntryByIsbnQuery(ValidIsbn);

        // Act
        await _handler.HandleAsync(query);

        // Assert
        await _catalogEntryRepository.Received(1).GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationToken()
    {
        // Arrange
        var catalogEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(catalogEntry);

        var query = new GetCatalogEntryByIsbnQuery(ValidIsbn);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.HandleAsync(query, token);

        // Assert
        await _catalogEntryRepository.Received(1).GetByIsbnAsync(Arg.Any<Isbn>(), token);
    }

    #endregion

    #region ISBN Normalization

    [Fact]
    public async Task HandleAsync_WithIsbnContainingHyphens_NormalizesIsbn()
    {
        // Arrange
        var catalogEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(catalogEntry);

        var query = new GetCatalogEntryByIsbnQuery("978-0-13-235088-4");

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Isbn.Should().Be("9780132350884");
    }

    [Fact]
    public async Task HandleAsync_WithIsbnContainingSpaces_NormalizesIsbn()
    {
        // Arrange
        var catalogEntry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        _catalogEntryRepository.GetByIsbnAsync(Arg.Any<Isbn>(), Arg.Any<CancellationToken>())
            .Returns(catalogEntry);

        var query = new GetCatalogEntryByIsbnQuery("978 0 13 235088 4");

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Isbn.Should().Be("9780132350884");
    }

    #endregion
}

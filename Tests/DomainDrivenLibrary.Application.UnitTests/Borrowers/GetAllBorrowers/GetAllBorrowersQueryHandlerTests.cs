using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.Borrowers.Shared;
using DomainDrivenLibrary.Borrowers.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DomainDrivenLibrary.Borrowers.GetAllBorrowers;

public class GetAllBorrowersQueryHandlerTests
{
    #region Test Data

    private static Borrower CreateBorrower(string id, string name, string email) =>
        Borrower.Register(BorrowerId.Create(id), name, EmailAddress.Create(email));

    #endregion

    #region Setup

    private readonly IBorrowerRepository _borrowerRepository;
    private readonly GetAllBorrowersQueryHandler _handler;

    public GetAllBorrowersQueryHandlerTests()
    {
        _borrowerRepository = Substitute.For<IBorrowerRepository>();
        _borrowerRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Borrower>());

        _handler = new GetAllBorrowersQueryHandler(_borrowerRepository);
    }

    #endregion

    #region Empty Results

    [Fact]
    public async Task HandleAsync_WhenNoBorrowers_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllBorrowersQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region Single Borrower

    [Fact]
    public async Task HandleAsync_WithSingleBorrower_ReturnsCorrectDto()
    {
        // Arrange
        var borrower = CreateBorrower("borrower-123", "John Doe", "john@example.com");
        _borrowerRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Borrower> { borrower });

        var query = new GetAllBorrowersQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.Id.Should().Be("BORROWER-123");
        dto.Name.Should().Be("John Doe");
        dto.Email.Should().Be("john@example.com");
    }

    #endregion

    #region Multiple Borrowers

    [Fact]
    public async Task HandleAsync_WithMultipleBorrowers_ReturnsAllBorrowers()
    {
        // Arrange
        var borrower1 = CreateBorrower("borrower-1", "John Doe", "john@example.com");
        var borrower2 = CreateBorrower("borrower-2", "Jane Smith", "jane@example.com");
        var borrower3 = CreateBorrower("borrower-3", "Bob Wilson", "bob@example.com");

        _borrowerRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Borrower> { borrower1, borrower2, borrower3 });

        var query = new GetAllBorrowersQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(dto => dto.Name == "John Doe");
        result.Should().Contain(dto => dto.Name == "Jane Smith");
        result.Should().Contain(dto => dto.Name == "Bob Wilson");
    }

    #endregion

    #region DTO Mapping

    [Fact]
    public async Task HandleAsync_MapsBorrowerFieldsCorrectly()
    {
        // Arrange
        var borrower = CreateBorrower("test-id", "Test Name", "test@example.com");
        _borrowerRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Borrower> { borrower });

        var query = new GetAllBorrowersQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result[0].Id.Should().Be("TEST-ID");
        result[0].Name.Should().Be("Test Name");
        result[0].Email.Should().Be("test@example.com");
    }

    #endregion

    #region Return Type

    [Fact]
    public async Task HandleAsync_ReturnsReadOnlyList()
    {
        // Arrange
        var query = new GetAllBorrowersQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeAssignableTo<IReadOnlyList<BorrowerDto>>();
    }

    #endregion

    #region Repository Interaction

    [Fact]
    public async Task HandleAsync_CallsRepository()
    {
        // Arrange
        var query = new GetAllBorrowersQuery();

        // Act
        await _handler.HandleAsync(query);

        // Assert
        await _borrowerRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationToken()
    {
        // Arrange
        var query = new GetAllBorrowersQuery();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.HandleAsync(query, token);

        // Assert
        await _borrowerRepository.Received(1).GetAllAsync(token);
    }

    #endregion
}

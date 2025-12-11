using DomainDrivenLibrary.Borrowers.ValueObjects;
using DomainDrivenLibrary.Data;
using DomainDrivenLibrary.Identifier;
using FluentAssertions;
using NSubstitute;

namespace DomainDrivenLibrary.Borrowers.RegisterBorrower;

public class RegisterBorrowerCommandHandlerTests
{
    #region Test Data

    private const string ValidName = "John Doe";
    private const string ValidEmail = "john.doe@example.com";
    private const string GeneratedId = "01HGXYZ123456";

    #endregion

    #region Setup

    private readonly IBorrowerRepository _borrowerRepository;
    private readonly IIdGenerator _idGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RegisterBorrowerCommandHandler _handler;

    public RegisterBorrowerCommandHandlerTests()
    {
        _borrowerRepository = Substitute.For<IBorrowerRepository>();
        _idGenerator = Substitute.For<IIdGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _idGenerator.New().Returns(GeneratedId);
        _borrowerRepository.ExistsByEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _handler = new RegisterBorrowerCommandHandler(
            _borrowerRepository,
            _idGenerator,
            _unitOfWork);
    }

    #endregion

    #region Success Scenarios

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsBorrowerDto()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(ValidName, ValidEmail);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(GeneratedId.ToUpperInvariant());
        result.Name.Should().Be(ValidName);
        result.Email.Should().Be(ValidEmail.ToLowerInvariant());
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_AddsBorrowerToRepository()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(ValidName, ValidEmail);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _borrowerRepository.Received(1).Add(Arg.Is<Borrower>(b =>
            b.Name == ValidName &&
            b.EmailAddress.Value == ValidEmail.ToLowerInvariant()));
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_SavesChanges()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(ValidName, ValidEmail);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_GeneratesNewId()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(ValidName, ValidEmail);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _idGenerator.Received(1).New();
    }

    [Fact]
    public async Task HandleAsync_NormalizesEmailToLowercase()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(ValidName, "JOHN.DOE@EXAMPLE.COM");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Email.Should().Be("john.doe@example.com");
    }

    #endregion

    #region Email Uniqueness Validation

    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(ValidName, ValidEmail);
        _borrowerRepository.ExistsByEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already registered*");
    }

    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyExists_DoesNotAddBorrower()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(ValidName, ValidEmail);
        _borrowerRepository.ExistsByEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        try { await _handler.HandleAsync(command); }
        catch {}

        // Assert
        _borrowerRepository.DidNotReceive().Add(Arg.Any<Borrower>());
    }

    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyExists_DoesNotSaveChanges()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(ValidName, ValidEmail);
        _borrowerRepository.ExistsByEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        try { await _handler.HandleAsync(command); }
        catch {}

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ChecksEmailUniquenessBeforeGeneratingId()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(ValidName, ValidEmail);
        _borrowerRepository.ExistsByEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        try { await _handler.HandleAsync(command); }
        catch {}

        // Assert
        _idGenerator.DidNotReceive().New();
    }

    #endregion

    #region Invalid Input Scenarios

    [Fact]
    public async Task HandleAsync_WithInvalidEmail_ThrowsArgumentException()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(ValidName, "invalid-email");

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*email*");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var command = new RegisterBorrowerCommand("", ValidEmail);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Name*");
    }

    [Fact]
    public async Task HandleAsync_WithWhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        var command = new RegisterBorrowerCommand("   ", ValidEmail);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithEmptyOrNullEmail_ThrowsArgumentException(string? email)
    {
        // Arrange
        var command = new RegisterBorrowerCommand(ValidName, email!);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region Cancellation Token

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToRepository()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(ValidName, ValidEmail);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.HandleAsync(command, token);

        // Assert
        await _borrowerRepository.Received(1)
            .ExistsByEmailAsync(Arg.Any<EmailAddress>(), token);
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToUnitOfWork()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(ValidName, ValidEmail);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.HandleAsync(command, token);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }

    #endregion
}
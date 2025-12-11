using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.Borrowers.ValueObjects;
using FluentAssertions;

namespace DomainDrivenLibrary.Borrowers;

public class BorrowerTests
{
    #region Test Data

    private static BorrowerId ValidBorrowerId => BorrowerId.Create("borrower-123");
    private const string ValidName = "John Doe";
    private static EmailAddress ValidEmail => EmailAddress.Create("john.doe@example.com");

    #endregion

    #region Register - Valid Input

    [Fact]
    public void Register_WithValidInput_ReturnsBorrower()
    {
        // Arrange
        BorrowerId id = ValidBorrowerId;
        EmailAddress email = ValidEmail;

        // Act
        Borrower borrower = Borrower.Register(id, ValidName, email);

        // Assert
        borrower.Should().NotBeNull();
        borrower.Id.Should().Be(id);
        borrower.Name.Should().Be(ValidName);
        borrower.EmailAddress.Should().Be(email);
    }

    [Fact]
    public void Register_WithDifferentEmails_CreatesBorrowersWithDifferentEmails()
    {
        // Arrange
        EmailAddress email1 = EmailAddress.Create("user1@example.com");
        EmailAddress email2 = EmailAddress.Create("user2@example.com");

        // Act
        Borrower borrower1 = Borrower.Register(BorrowerId.Create("id-1"), "User 1", email1);
        Borrower borrower2 = Borrower.Register(BorrowerId.Create("id-2"), "User 2", email2);

        // Assert
        borrower1.EmailAddress.Should().NotBe(borrower2.EmailAddress);
    }

    #endregion

    #region Register - Invalid Name

    [Fact]
    public void Register_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        string? name = null;

        // Act
        Action act = () => Borrower.Register(ValidBorrowerId, name!, ValidEmail);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name")
            .WithMessage("*Name must not be empty*");
    }

    [Fact]
    public void Register_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        const string name = "";

        // Act
        Action act = () => Borrower.Register(ValidBorrowerId, name, ValidEmail);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name")
            .WithMessage("*Name must not be empty*");
    }

    [Fact]
    public void Register_WithWhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        const string name = "   ";

        // Act
        Action act = () => Borrower.Register(ValidBorrowerId, name, ValidEmail);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name")
            .WithMessage("*Name must not be empty*");
    }

    #endregion

    #region UpdateName

    [Fact]
    public void UpdateName_WithDifferentValue_UpdatesName()
    {
        // Arrange
        Borrower borrower = Borrower.Register(ValidBorrowerId, ValidName, ValidEmail);
        const string newName = "Jane Doe";

        // Act
        borrower.UpdateName(newName);

        // Assert
        borrower.Name.Should().Be(newName);
    }

    [Fact]
    public void UpdateName_WithSameValue_DoesNotChangeName()
    {
        // Arrange
        Borrower borrower = Borrower.Register(ValidBorrowerId, ValidName, ValidEmail);

        // Act
        borrower.UpdateName(ValidName);

        // Assert
        borrower.Name.Should().Be(ValidName);
    }

    [Fact]
    public void UpdateName_ReturnsSameInstance()
    {
        // Arrange
        Borrower borrower = Borrower.Register(ValidBorrowerId, ValidName, ValidEmail);

        // Act
        Borrower result = borrower.UpdateName("New Name");

        // Assert
        result.Should().BeSameAs(borrower);
    }

    #endregion

    #region UpdateEmailAddress

    [Fact]
    public void UpdateEmailAddress_WithDifferentValue_UpdatesEmailAddress()
    {
        // Arrange
        Borrower borrower = Borrower.Register(ValidBorrowerId, ValidName, ValidEmail);
        EmailAddress newEmail = EmailAddress.Create("new.email@example.com");

        // Act
        borrower.UpdateEmailAddress(newEmail);

        // Assert
        borrower.EmailAddress.Should().Be(newEmail);
    }

    [Fact]
    public void UpdateEmailAddress_WithSameValue_DoesNotChangeEmailAddress()
    {
        // Arrange
        Borrower borrower = Borrower.Register(ValidBorrowerId, ValidName, ValidEmail);
        EmailAddress sameEmail = EmailAddress.Create("john.doe@example.com");

        // Act
        borrower.UpdateEmailAddress(sameEmail);

        // Assert
        borrower.EmailAddress.Should().Be(ValidEmail);
    }

    [Fact]
    public void UpdateEmailAddress_ReturnsSameInstance()
    {
        // Arrange
        Borrower borrower = Borrower.Register(ValidBorrowerId, ValidName, ValidEmail);
        EmailAddress newEmail = EmailAddress.Create("new@example.com");

        // Act
        Borrower result = borrower.UpdateEmailAddress(newEmail);

        // Assert
        result.Should().BeSameAs(borrower);
    }

    #endregion

    #region Fluent Chaining

    [Fact]
    public void UpdateMethods_CanBeChained()
    {
        // Arrange
        Borrower borrower = Borrower.Register(ValidBorrowerId, ValidName, ValidEmail);
        const string newName = "Jane Smith";
        EmailAddress newEmail = EmailAddress.Create("jane.smith@example.com");

        // Act
        borrower
            .UpdateName(newName)
            .UpdateEmailAddress(newEmail);

        // Assert
        borrower.Name.Should().Be(newName);
        borrower.EmailAddress.Should().Be(newEmail);
    }

    #endregion

    #region Identity

    [Fact]
    public void Id_IsSetCorrectlyOnCreation()
    {
        // Arrange
        BorrowerId expectedId = ValidBorrowerId;

        // Act
        Borrower borrower = Borrower.Register(expectedId, ValidName, ValidEmail);

        // Assert
        borrower.Id.Should().Be(expectedId);
    }

    [Fact]
    public void TwoBorrowersWithDifferentIds_AreDifferentEntities()
    {
        // Arrange
        BorrowerId id1 = BorrowerId.Create("borrower-1");
        BorrowerId id2 = BorrowerId.Create("borrower-2");

        // Act
        Borrower borrower1 = Borrower.Register(id1, ValidName, ValidEmail);
        Borrower borrower2 = Borrower.Register(id2, ValidName, ValidEmail);

        // Assert
        borrower1.Id.Should().NotBe(borrower2.Id);
    }

    #endregion
}

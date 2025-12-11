using DomainDrivenLibrary.Borrowers.ValueObjects;
using FluentAssertions;

namespace DomainDrivenLibrary.Borrowers.ValueObjects;

public class EmailAddressTests
{
    #region Create - Valid Input

    [Fact]
    public void Create_WithValidEmail_ReturnsEmailAddress()
    {
        // Arrange
        const string email = "test@example.com";

        // Act
        EmailAddress emailAddress = EmailAddress.Create(email);

        // Assert
        emailAddress.Should().NotBeNull();
        emailAddress.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_WithUppercaseEmail_ReturnsLowercaseValue()
    {
        // Arrange
        const string email = "TEST@EXAMPLE.COM";

        // Act
        EmailAddress emailAddress = EmailAddress.Create(email);

        // Assert
        emailAddress.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_WithMixedCaseEmail_ReturnsLowercaseValue()
    {
        // Arrange
        const string email = "John.Doe@Example.COM";

        // Act
        EmailAddress emailAddress = EmailAddress.Create(email);

        // Assert
        emailAddress.Value.Should().Be("john.doe@example.com");
    }

    [Fact]
    public void Create_WithSubdomain_ReturnsEmailAddress()
    {
        // Arrange
        const string email = "user@mail.example.com";

        // Act
        EmailAddress emailAddress = EmailAddress.Create(email);

        // Assert
        emailAddress.Value.Should().Be("user@mail.example.com");
    }

    [Fact]
    public void Create_WithPlusSign_ReturnsEmailAddress()
    {
        // Arrange
        const string email = "user+tag@example.com";

        // Act
        EmailAddress emailAddress = EmailAddress.Create(email);

        // Assert
        emailAddress.Value.Should().Be("user+tag@example.com");
    }

    [Fact]
    public void Create_WithNumbers_ReturnsEmailAddress()
    {
        // Arrange
        const string email = "user123@example456.com";

        // Act
        EmailAddress emailAddress = EmailAddress.Create(email);

        // Assert
        emailAddress.Value.Should().Be("user123@example456.com");
    }

    #endregion

    #region Create - Invalid Input

    [Fact]
    public void Create_WithNull_ThrowsArgumentException()
    {
        // Arrange
        string? email = null;

        // Act
        Action act = () => EmailAddress.Create(email!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        const string email = "";

        // Act
        Action act = () => EmailAddress.Create(email);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespace_ThrowsArgumentException()
    {
        // Arrange
        const string email = "   ";

        // Act
        Action act = () => EmailAddress.Create(email);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithoutAtSymbol_ThrowsArgumentException()
    {
        // Arrange
        const string email = "invalidemail.com";

        // Act
        Action act = () => EmailAddress.Create(email);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid email format*");
    }

    [Fact]
    public void Create_WithoutDomain_ThrowsArgumentException()
    {
        // Arrange
        const string email = "user@";

        // Act
        Action act = () => EmailAddress.Create(email);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithoutLocalPart_ThrowsArgumentException()
    {
        // Arrange
        const string email = "@example.com";

        // Act
        Action act = () => EmailAddress.Create(email);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithMultipleAtSymbols_ThrowsArgumentException()
    {
        // Arrange
        const string email = "user@@example.com";

        // Act
        Action act = () => EmailAddress.Create(email);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region TryParse - Valid Input

    [Fact]
    public void TryParse_WithValidEmail_ReturnsTrueAndOutputsEmailAddress()
    {
        // Arrange
        const string email = "test@example.com";

        // Act
        bool result = EmailAddress.TryParse(email, out EmailAddress? emailAddress);

        // Assert
        result.Should().BeTrue();
        emailAddress.Should().NotBeNull();
        emailAddress!.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void TryParse_WithUppercaseEmail_NormalizesToLowercase()
    {
        // Arrange
        const string email = "TEST@EXAMPLE.COM";

        // Act
        bool result = EmailAddress.TryParse(email, out EmailAddress? emailAddress);

        // Assert
        result.Should().BeTrue();
        emailAddress!.Value.Should().Be("test@example.com");
    }

    #endregion

    #region TryParse - Invalid Input

    [Fact]
    public void TryParse_WithNull_ReturnsFalseAndOutputsNull()
    {
        // Arrange
        string? email = null;

        // Act
        bool result = EmailAddress.TryParse(email, out EmailAddress? emailAddress);

        // Assert
        result.Should().BeFalse();
        emailAddress.Should().BeNull();
    }

    [Fact]
    public void TryParse_WithEmptyString_ReturnsFalseAndOutputsNull()
    {
        // Arrange
        const string email = "";

        // Act
        bool result = EmailAddress.TryParse(email, out EmailAddress? emailAddress);

        // Assert
        result.Should().BeFalse();
        emailAddress.Should().BeNull();
    }

    [Fact]
    public void TryParse_WithInvalidFormat_ReturnsFalseAndOutputsNull()
    {
        // Arrange
        const string email = "invalid";

        // Act
        bool result = EmailAddress.TryParse(email, out EmailAddress? emailAddress);

        // Assert
        result.Should().BeFalse();
        emailAddress.Should().BeNull();
    }

    #endregion

    #region Value Equality

    [Fact]
    public void Equals_TwoEmailAddressesWithSameValue_ReturnsTrue()
    {
        // Arrange
        EmailAddress email1 = EmailAddress.Create("test@example.com");
        EmailAddress email2 = EmailAddress.Create("TEST@EXAMPLE.COM");

        // Act & Assert
        email1.Should().Be(email2);
    }

    [Fact]
    public void Equals_TwoEmailAddressesWithDifferentValues_ReturnsFalse()
    {
        // Arrange
        EmailAddress email1 = EmailAddress.Create("user1@example.com");
        EmailAddress email2 = EmailAddress.Create("user2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
    }

    [Fact]
    public void GetHashCode_TwoEmailAddressesWithSameValue_ReturnsSameHashCode()
    {
        // Arrange
        EmailAddress email1 = EmailAddress.Create("test@example.com");
        EmailAddress email2 = EmailAddress.Create("test@example.com");

        // Act & Assert
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    #endregion

    #region Conversions

    [Fact]
    public void ImplicitStringConversion_ReturnsValue()
    {
        // Arrange
        EmailAddress emailAddress = EmailAddress.Create("test@example.com");

        // Act
        string result = emailAddress;

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        EmailAddress emailAddress = EmailAddress.Create("test@example.com");

        // Act
        string result = emailAddress.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }

    #endregion
}

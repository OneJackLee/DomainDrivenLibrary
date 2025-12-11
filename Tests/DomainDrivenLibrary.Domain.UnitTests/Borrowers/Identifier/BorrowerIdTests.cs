using DomainDrivenLibrary.Borrowers.Identifier;
using FluentAssertions;

namespace DomainDrivenLibrary.Borrowers.Identifier;

public class BorrowerIdTests
{
    #region Create - Valid Input

    [Fact]
    public void Create_WithValidValue_ReturnsBorrowerId()
    {
        // Arrange
        const string value = "borrower-123";

        // Act
        BorrowerId borrowerId = BorrowerId.Create(value);

        // Assert
        borrowerId.Should().NotBeNull();
        borrowerId.Value.Should().Be("BORROWER-123");
    }

    [Fact]
    public void Create_WithLowercaseValue_ReturnsUppercaseValue()
    {
        // Arrange
        const string value = "abc-def-123";

        // Act
        BorrowerId borrowerId = BorrowerId.Create(value);

        // Assert
        borrowerId.Value.Should().Be("ABC-DEF-123");
    }

    [Fact]
    public void Create_WithMixedCaseValue_ReturnsUppercaseValue()
    {
        // Arrange
        const string value = "Borrower-Id-456";

        // Act
        BorrowerId borrowerId = BorrowerId.Create(value);

        // Assert
        borrowerId.Value.Should().Be("BORROWER-ID-456");
    }

    #endregion

    #region Create - Invalid Input

    [Fact]
    public void Create_WithNull_ThrowsArgumentException()
    {
        // Arrange
        string? value = null;

        // Act
        Action act = () => BorrowerId.Create(value!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        const string value = "";

        // Act
        Action act = () => BorrowerId.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespace_ThrowsArgumentException()
    {
        // Arrange
        const string value = "   ";

        // Act
        Action act = () => BorrowerId.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Value Equality

    [Fact]
    public void Equals_TwoBorrowerIdsWithSameValue_ReturnsTrue()
    {
        // Arrange
        BorrowerId id1 = BorrowerId.Create("borrower-123");
        BorrowerId id2 = BorrowerId.Create("BORROWER-123");

        // Act & Assert
        id1.Should().Be(id2);
    }

    [Fact]
    public void Equals_TwoBorrowerIdsWithDifferentValues_ReturnsFalse()
    {
        // Arrange
        BorrowerId id1 = BorrowerId.Create("borrower-123");
        BorrowerId id2 = BorrowerId.Create("borrower-456");

        // Act & Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void GetHashCode_TwoBorrowerIdsWithSameValue_ReturnsSameHashCode()
    {
        // Arrange
        BorrowerId id1 = BorrowerId.Create("borrower-123");
        BorrowerId id2 = BorrowerId.Create("borrower-123");

        // Act & Assert
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    #endregion

    #region Conversions

    [Fact]
    public void ImplicitStringConversion_ReturnsValue()
    {
        // Arrange
        BorrowerId borrowerId = BorrowerId.Create("borrower-123");

        // Act
        string result = borrowerId;

        // Assert
        result.Should().Be("BORROWER-123");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        BorrowerId borrowerId = BorrowerId.Create("borrower-123");

        // Act
        string result = borrowerId.ToString();

        // Assert
        result.Should().Be("BORROWER-123");
    }

    #endregion
}

using DomainDrivenLibrary.Books.Identifier;
using FluentAssertions;

namespace DomainDrivenLibrary.Books.Identifier;

public class BookIdTests
{
    #region Create - Valid Input

    [Fact]
    public void Create_WithValidValue_ReturnsBookId()
    {
        // Arrange
        const string value = "book-123";

        // Act
        BookId bookId = BookId.Create(value);

        // Assert
        bookId.Should().NotBeNull();
        bookId.Value.Should().Be("BOOK-123");
    }

    [Fact]
    public void Create_WithLowercaseValue_ReturnsUppercaseValue()
    {
        // Arrange
        const string value = "abc-def-123";

        // Act
        BookId bookId = BookId.Create(value);

        // Assert
        bookId.Value.Should().Be("ABC-DEF-123");
    }

    [Fact]
    public void Create_WithMixedCaseValue_ReturnsUppercaseValue()
    {
        // Arrange
        const string value = "Book-Id-456";

        // Act
        BookId bookId = BookId.Create(value);

        // Assert
        bookId.Value.Should().Be("BOOK-ID-456");
    }

    #endregion

    #region Create - Invalid Input

    [Fact]
    public void Create_WithNull_ThrowsArgumentException()
    {
        // Arrange
        string? value = null;

        // Act
        Action act = () => BookId.Create(value!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        const string value = "";

        // Act
        Action act = () => BookId.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespace_ThrowsArgumentException()
    {
        // Arrange
        const string value = "   ";

        // Act
        Action act = () => BookId.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Value Equality

    [Fact]
    public void Equals_TwoBookIdsWithSameValue_ReturnsTrue()
    {
        // Arrange
        BookId id1 = BookId.Create("book-123");
        BookId id2 = BookId.Create("BOOK-123");

        // Act & Assert
        id1.Should().Be(id2);
    }

    [Fact]
    public void Equals_TwoBookIdsWithDifferentValues_ReturnsFalse()
    {
        // Arrange
        BookId id1 = BookId.Create("book-123");
        BookId id2 = BookId.Create("book-456");

        // Act & Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void GetHashCode_TwoBookIdsWithSameValue_ReturnsSameHashCode()
    {
        // Arrange
        BookId id1 = BookId.Create("book-123");
        BookId id2 = BookId.Create("book-123");

        // Act & Assert
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    #endregion

    #region Conversions

    [Fact]
    public void ImplicitStringConversion_ReturnsValue()
    {
        // Arrange
        BookId bookId = BookId.Create("book-123");

        // Act
        string result = bookId;

        // Assert
        result.Should().Be("BOOK-123");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        BookId bookId = BookId.Create("book-123");

        // Act
        string result = bookId.ToString();

        // Assert
        result.Should().Be("BOOK-123");
    }

    #endregion
}

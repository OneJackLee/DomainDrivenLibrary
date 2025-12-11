using FluentAssertions;

namespace DomainDrivenLibrary.CatalogEntries.ValueObjects;

public class IsbnTests
{
    #region Create - Valid Input

    [Fact]
    public void Create_WithValidIsbn10_ReturnsIsbnWithNormalizedValue()
    {
        // Arrange
        const string input = "0132350882";

        // Act
        Isbn isbn = Isbn.Create(input);

        // Assert
        isbn.Value.Should().Be("0132350882");
    }

    [Fact]
    public void Create_WithValidIsbn13_ReturnsIsbnWithNormalizedValue()
    {
        // Arrange
        const string input = "9780132350884";

        // Act
        Isbn isbn = Isbn.Create(input);

        // Assert
        isbn.Value.Should().Be("9780132350884");
    }

    [Fact]
    public void Create_WithIsbn10EndingInX_ReturnsIsbnWithUppercaseX()
    {
        // Arrange
        const string input = "080442957x";

        // Act
        Isbn isbn = Isbn.Create(input);

        // Assert
        isbn.Value.Should().Be("080442957X");
    }

    [Fact]
    public void Create_WithHyphens_ReturnsIsbnWithHyphensRemoved()
    {
        // Arrange
        const string input = "978-0-13-235088-4";

        // Act
        Isbn isbn = Isbn.Create(input);

        // Assert
        isbn.Value.Should().Be("9780132350884");
    }

    [Fact]
    public void Create_WithSpaces_ReturnsIsbnWithSpacesRemoved()
    {
        // Arrange
        const string input = "978 0 13 235088 4";

        // Act
        Isbn isbn = Isbn.Create(input);

        // Assert
        isbn.Value.Should().Be("9780132350884");
    }

    [Fact]
    public void Create_WithMixedHyphensAndSpaces_ReturnsNormalizedIsbn()
    {
        // Arrange
        const string input = "978-0 13-235088 4";

        // Act
        Isbn isbn = Isbn.Create(input);

        // Assert
        isbn.Value.Should().Be("9780132350884");
    }

    #endregion

    #region Create - Invalid Input

    [Fact]
    public void Create_WithNull_ThrowsArgumentException()
    {
        // Arrange
        string? input = null;

        // Act
        Action act = () => Isbn.Create(input!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        const string input = "";

        // Act
        Action act = () => Isbn.Create(input);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespace_ThrowsArgumentException()
    {
        // Arrange
        const string input = "   ";

        // Act
        Action act = () => Isbn.Create(input);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithTooFewDigits_ThrowsArgumentException()
    {
        // Arrange
        const string input = "123456789"; // 9 digits

        // Act
        Action act = () => Isbn.Create(input);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*9 digits*");
    }

    [Fact]
    public void Create_WithTooManyDigits_ThrowsArgumentException()
    {
        // Arrange
        const string input = "12345678901234"; // 14 digits

        // Act
        Action act = () => Isbn.Create(input);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*14 digits*");
    }

    [Fact]
    public void Create_WithElevenDigits_ThrowsArgumentException()
    {
        // Arrange
        const string input = "12345678901"; // 11 digits (between 10 and 13)

        // Act
        Action act = () => Isbn.Create(input);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithLettersInIsbn13_ThrowsArgumentException()
    {
        // Arrange
        const string input = "978013235088A"; // letter in ISBN-13

        // Act
        Action act = () => Isbn.Create(input);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithXInMiddleOfIsbn10_ThrowsArgumentException()
    {
        // Arrange
        const string input = "01323X0882"; // X not at end

        // Act
        Action act = () => Isbn.Create(input);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region TryParse - Valid Input

    [Fact]
    public void TryParse_WithValidIsbn10_ReturnsTrueAndOutputsIsbn()
    {
        // Arrange
        const string input = "0132350882";

        // Act
        bool result = Isbn.TryParse(input, out Isbn? isbn);

        // Assert
        result.Should().BeTrue();
        isbn.Should().NotBeNull();
        isbn!.Value.Should().Be("0132350882");
    }

    [Fact]
    public void TryParse_WithValidIsbn13_ReturnsTrueAndOutputsIsbn()
    {
        // Arrange
        const string input = "9780132350884";

        // Act
        bool result = Isbn.TryParse(input, out Isbn? isbn);

        // Assert
        result.Should().BeTrue();
        isbn.Should().NotBeNull();
        isbn!.Value.Should().Be("9780132350884");
    }

    [Fact]
    public void TryParse_WithHyphens_ReturnsTrueAndNormalizesValue()
    {
        // Arrange
        const string input = "978-0-13-235088-4";

        // Act
        bool result = Isbn.TryParse(input, out Isbn? isbn);

        // Assert
        result.Should().BeTrue();
        isbn!.Value.Should().Be("9780132350884");
    }

    #endregion

    #region TryParse - Invalid Input

    [Fact]
    public void TryParse_WithNull_ReturnsFalseAndOutputsNull()
    {
        // Arrange
        string? input = null;

        // Act
        bool result = Isbn.TryParse(input, out Isbn? isbn);

        // Assert
        result.Should().BeFalse();
        isbn.Should().BeNull();
    }

    [Fact]
    public void TryParse_WithEmptyString_ReturnsFalseAndOutputsNull()
    {
        // Arrange
        const string input = "";

        // Act
        bool result = Isbn.TryParse(input, out Isbn? isbn);

        // Assert
        result.Should().BeFalse();
        isbn.Should().BeNull();
    }

    [Fact]
    public void TryParse_WithInvalidFormat_ReturnsFalseAndOutputsNull()
    {
        // Arrange
        const string input = "invalid";

        // Act
        bool result = Isbn.TryParse(input, out Isbn? isbn);

        // Assert
        result.Should().BeFalse();
        isbn.Should().BeNull();
    }

    [Fact]
    public void TryParse_WithWrongLength_ReturnsFalseAndOutputsNull()
    {
        // Arrange
        const string input = "123456789"; // 9 digits

        // Act
        bool result = Isbn.TryParse(input, out Isbn? isbn);

        // Assert
        result.Should().BeFalse();
        isbn.Should().BeNull();
    }

    #endregion

    #region Value Equality

    [Fact]
    public void Equals_TwoIsbnsWithSameValue_ReturnsTrue()
    {
        // Arrange
        Isbn isbn1 = Isbn.Create("9780132350884");
        Isbn isbn2 = Isbn.Create("978-0-13-235088-4");

        // Act & Assert
        isbn1.Should().Be(isbn2);
    }

    [Fact]
    public void Equals_TwoIsbnsWithDifferentValues_ReturnsFalse()
    {
        // Arrange
        Isbn isbn1 = Isbn.Create("9780132350884");
        Isbn isbn2 = Isbn.Create("0132350882");

        // Act & Assert
        isbn1.Should().NotBe(isbn2);
    }

    [Fact]
    public void GetHashCode_TwoIsbnsWithSameValue_ReturnsSameHashCode()
    {
        // Arrange
        Isbn isbn1 = Isbn.Create("9780132350884");
        Isbn isbn2 = Isbn.Create("978-0-13-235088-4");

        // Act & Assert
        isbn1.GetHashCode().Should().Be(isbn2.GetHashCode());
    }

    #endregion

    #region Conversions

    [Fact]
    public void ImplicitStringConversion_ReturnsValue()
    {
        // Arrange
        Isbn isbn = Isbn.Create("9780132350884");

        // Act
        string result = isbn;

        // Assert
        result.Should().Be("9780132350884");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        Isbn isbn = Isbn.Create("9780132350884");

        // Act
        string result = isbn.ToString();

        // Assert
        result.Should().Be("9780132350884");
    }

    #endregion
}
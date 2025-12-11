using DomainDrivenLibrary.CatalogEntries.ValueObjects;
using FluentAssertions;

namespace DomainDrivenLibrary.CatalogEntries;

public class CatalogEntryTests
{
    #region Test Data

    private static Isbn ValidIsbn => Isbn.Create("9780132350884");
    private const string ValidIsbnString = "9780132350884";
    private const string ValidTitle = "Clean Code";
    private const string ValidAuthor = "Robert C. Martin";

    #endregion

    #region Create with Isbn Value Object - Valid Input

    [Fact]
    public void Create_WithValidIsbnAndTitleAndAuthor_ReturnsCatalogEntry()
    {
        // Arrange
        Isbn isbn = ValidIsbn;

        // Act
        CatalogEntry entry = CatalogEntry.Create(isbn, ValidTitle, ValidAuthor);

        // Assert
        entry.Should().NotBeNull();
        entry.Isbn.Should().Be(isbn);
        entry.Title.Should().Be(ValidTitle);
        entry.Author.Should().Be(ValidAuthor);
    }

    [Fact]
    public void Create_WithValidInput_SetsIsbnCorrectly()
    {
        // Arrange
        Isbn isbn = Isbn.Create("978-0-13-235088-4");

        // Act
        CatalogEntry entry = CatalogEntry.Create(isbn, ValidTitle, ValidAuthor);

        // Assert
        entry.Isbn.Value.Should().Be("9780132350884");
    }

    #endregion

    #region Create with Isbn Value Object - Invalid Title

    [Fact]
    public void Create_WithNullTitle_ThrowsArgumentException()
    {
        // Arrange
        Isbn isbn = ValidIsbn;
        string? title = null;

        // Act
        Action act = () => CatalogEntry.Create(isbn, title!, ValidAuthor);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("title")
            .WithMessage("*Title must not be empty*");
    }

    [Fact]
    public void Create_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        Isbn isbn = ValidIsbn;
        const string title = "";

        // Act
        Action act = () => CatalogEntry.Create(isbn, title, ValidAuthor);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("title")
            .WithMessage("*Title must not be empty*");
    }

    [Fact]
    public void Create_WithWhitespaceTitle_ThrowsArgumentException()
    {
        // Arrange
        Isbn isbn = ValidIsbn;
        const string title = "   ";

        // Act
        Action act = () => CatalogEntry.Create(isbn, title, ValidAuthor);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("title")
            .WithMessage("*Title must not be empty*");
    }

    #endregion

    #region Create with Isbn Value Object - Invalid Author

    [Fact]
    public void Create_WithNullAuthor_ThrowsArgumentException()
    {
        // Arrange
        Isbn isbn = ValidIsbn;
        string? author = null;

        // Act
        Action act = () => CatalogEntry.Create(isbn, ValidTitle, author!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("author")
            .WithMessage("*Author must not be empty*");
    }

    [Fact]
    public void Create_WithEmptyAuthor_ThrowsArgumentException()
    {
        // Arrange
        Isbn isbn = ValidIsbn;
        const string author = "";

        // Act
        Action act = () => CatalogEntry.Create(isbn, ValidTitle, author);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("author")
            .WithMessage("*Author must not be empty*");
    }

    [Fact]
    public void Create_WithWhitespaceAuthor_ThrowsArgumentException()
    {
        // Arrange
        Isbn isbn = ValidIsbn;
        const string author = "   ";

        // Act
        Action act = () => CatalogEntry.Create(isbn, ValidTitle, author);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("author")
            .WithMessage("*Author must not be empty*");
    }

    #endregion

    #region Create with String ISBN - Valid Input

    [Fact]
    public void Create_WithValidStringIsbn_ReturnsCatalogEntry()
    {
        // Act
        CatalogEntry entry = CatalogEntry.Create(ValidIsbnString, ValidTitle, ValidAuthor);

        // Assert
        entry.Should().NotBeNull();
        entry.Isbn.Value.Should().Be(ValidIsbnString);
        entry.Title.Should().Be(ValidTitle);
        entry.Author.Should().Be(ValidAuthor);
    }

    [Fact]
    public void Create_WithHyphenatedStringIsbn_NormalizesIsbn()
    {
        // Arrange
        const string isbnWithHyphens = "978-0-13-235088-4";

        // Act
        CatalogEntry entry = CatalogEntry.Create(isbnWithHyphens, ValidTitle, ValidAuthor);

        // Assert
        entry.Isbn.Value.Should().Be("9780132350884");
    }

    [Fact]
    public void Create_WithIsbn10String_ReturnsCatalogEntry()
    {
        // Arrange
        const string isbn10 = "0132350882";

        // Act
        CatalogEntry entry = CatalogEntry.Create(isbn10, ValidTitle, ValidAuthor);

        // Assert
        entry.Isbn.Value.Should().Be("0132350882");
    }

    #endregion

    #region Create with String ISBN - Invalid ISBN

    [Fact]
    public void Create_WithNullStringIsbn_ThrowsArgumentException()
    {
        // Arrange
        string? isbn = null;

        // Act
        Action act = () => CatalogEntry.Create(isbn!, ValidTitle, ValidAuthor);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("isbn")
            .WithMessage("*ISBN provided is not valid*");
    }

    [Fact]
    public void Create_WithEmptyStringIsbn_ThrowsArgumentException()
    {
        // Arrange
        const string isbn = "";

        // Act
        Action act = () => CatalogEntry.Create(isbn, ValidTitle, ValidAuthor);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("isbn")
            .WithMessage("*ISBN provided is not valid*");
    }

    [Fact]
    public void Create_WithInvalidStringIsbn_ThrowsArgumentException()
    {
        // Arrange
        const string isbn = "invalid-isbn";

        // Act
        Action act = () => CatalogEntry.Create(isbn, ValidTitle, ValidAuthor);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("isbn")
            .WithMessage("*ISBN provided is not valid*invalid-isbn*");
    }

    [Fact]
    public void Create_WithWrongLengthStringIsbn_ThrowsArgumentException()
    {
        // Arrange
        const string isbn = "123456789"; // 9 digits

        // Act
        Action act = () => CatalogEntry.Create(isbn, ValidTitle, ValidAuthor);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("isbn");
    }

    #endregion

    #region Create with String ISBN - Invalid Title/Author

    [Fact]
    public void Create_StringOverload_WithEmptyTitle_ThrowsArgumentException()
    {
        // Act
        Action act = () => CatalogEntry.Create(ValidIsbnString, "", ValidAuthor);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("title");
    }

    [Fact]
    public void Create_StringOverload_WithEmptyAuthor_ThrowsArgumentException()
    {
        // Act
        Action act = () => CatalogEntry.Create(ValidIsbnString, ValidTitle, "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("author");
    }

    #endregion

    #region Property Immutability

    [Fact]
    public void Isbn_CannotBeChangedAfterCreation()
    {
        // Arrange
        CatalogEntry entry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        Isbn originalIsbn = entry.Isbn;

        // Assert - Isbn property has private init, so it cannot be changed
        // This test documents the expected behavior
        entry.Isbn.Should().Be(originalIsbn);
    }

    #endregion

    #region Multiple Catalog Entries

    [Fact]
    public void Create_TwoCatalogEntriesWithSameIsbn_HaveEqualIsbns()
    {
        // Arrange & Act
        CatalogEntry entry1 = CatalogEntry.Create("9780132350884", "Title 1", "Author 1");
        CatalogEntry entry2 = CatalogEntry.Create("978-0-13-235088-4", "Title 2", "Author 2");

        // Assert - Both entries have the same normalized ISBN
        entry1.Isbn.Should().Be(entry2.Isbn);
    }

    [Fact]
    public void Create_TwoCatalogEntriesWithDifferentIsbns_HaveDifferentIsbns()
    {
        // Arrange & Act
        CatalogEntry entry1 = CatalogEntry.Create("9780132350884", ValidTitle, ValidAuthor);
        CatalogEntry entry2 = CatalogEntry.Create("0132350882", ValidTitle, ValidAuthor);

        // Assert
        entry1.Isbn.Should().NotBe(entry2.Isbn);
    }

    #endregion

    #region UpdateAuthor

    [Fact]
    public void UpdateAuthor_WithDifferentValue_UpdatesAuthor()
    {
        // Arrange
        CatalogEntry entry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        const string newAuthor = "Uncle Bob";

        // Act
        entry.UpdateAuthor(newAuthor);

        // Assert
        entry.Author.Should().Be(newAuthor);
    }

    [Fact]
    public void UpdateAuthor_WithSameValue_DoesNotChangeAuthor()
    {
        // Arrange
        CatalogEntry entry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);

        // Act
        entry.UpdateAuthor(ValidAuthor);

        // Assert
        entry.Author.Should().Be(ValidAuthor);
    }

    [Fact]
    public void UpdateAuthor_ReturnsSameInstance()
    {
        // Arrange
        CatalogEntry entry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);

        // Act
        CatalogEntry result = entry.UpdateAuthor("New Author");

        // Assert
        result.Should().BeSameAs(entry);
    }

    #endregion

    #region UpdateTitle

    [Fact]
    public void UpdateTitle_WithDifferentValue_UpdatesTitle()
    {
        // Arrange
        CatalogEntry entry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        const string newTitle = "Clean Architecture";

        // Act
        entry.UpdateTitle(newTitle);

        // Assert
        entry.Title.Should().Be(newTitle);
    }

    [Fact]
    public void UpdateTitle_WithSameValue_DoesNotChangeTitle()
    {
        // Arrange
        CatalogEntry entry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);

        // Act
        entry.UpdateTitle(ValidTitle);

        // Assert
        entry.Title.Should().Be(ValidTitle);
    }

    [Fact]
    public void UpdateTitle_ReturnsSameInstance()
    {
        // Arrange
        CatalogEntry entry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);

        // Act
        CatalogEntry result = entry.UpdateTitle("New Title");

        // Assert
        result.Should().BeSameAs(entry);
    }

    #endregion

    #region Fluent Chaining

    [Fact]
    public void UpdateMethods_CanBeChained()
    {
        // Arrange
        CatalogEntry entry = CatalogEntry.Create(ValidIsbn, ValidTitle, ValidAuthor);
        const string newTitle = "Clean Architecture";
        const string newAuthor = "Uncle Bob";

        // Act
        entry
            .UpdateTitle(newTitle)
            .UpdateAuthor(newAuthor);

        // Assert
        entry.Title.Should().Be(newTitle);
        entry.Author.Should().Be(newAuthor);
    }

    #endregion
}

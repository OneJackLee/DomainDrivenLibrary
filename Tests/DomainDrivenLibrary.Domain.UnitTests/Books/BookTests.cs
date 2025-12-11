using DomainDrivenLibrary.Books.Identifier;
using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.CatalogEntries.ValueObjects;
using FluentAssertions;

namespace DomainDrivenLibrary.Books;

public class BookTests
{
    #region Test Data

    private static BookId ValidBookId => BookId.Create("book-123");
    private static Isbn ValidIsbn => Isbn.Create("9780132350884");
    private static BorrowerId ValidBorrowerId => BorrowerId.Create("borrower-456");

    #endregion

    #region Register

    [Fact]
    public void Register_WithValidIdAndIsbn_ReturnsBook()
    {
        // Arrange
        BookId id = ValidBookId;
        Isbn isbn = ValidIsbn;

        // Act
        Book book = Book.Register(id, isbn);

        // Assert
        book.Should().NotBeNull();
        book.Id.Should().Be(id);
        book.Isbn.Should().Be(isbn);
    }

    [Fact]
    public void Register_ReturnsBookInAvailableState()
    {
        // Arrange & Act
        Book book = Book.Register(ValidBookId, ValidIsbn);

        // Assert
        book.IsAvailable.Should().BeTrue();
        book.BorrowerId.Should().BeNull();
        book.BorrowedOn.Should().BeNull();
    }

    #endregion

    #region Borrow - Valid Scenarios

    [Fact]
    public void Borrow_WhenAvailable_SetsBorrowerIdAndBorrowedOn()
    {
        // Arrange
        Book book = Book.Register(ValidBookId, ValidIsbn);
        BorrowerId borrowerId = ValidBorrowerId;
        DateTime borrowedOn = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        book.Borrow(borrowerId, borrowedOn);

        // Assert
        book.BorrowerId.Should().Be(borrowerId);
        book.BorrowedOn.Should().Be(borrowedOn);
        book.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Borrow_WithoutBorrowedOnDate_UsesUtcNow()
    {
        // Arrange
        Book book = Book.Register(ValidBookId, ValidIsbn);
        DateTime beforeBorrow = DateTime.UtcNow;

        // Act
        book.Borrow(ValidBorrowerId);

        // Assert
        book.BorrowedOn.Should().NotBeNull();
        book.BorrowedOn.Should().BeOnOrAfter(beforeBorrow);
        book.BorrowedOn.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Borrow_ReturnsSameInstance()
    {
        // Arrange
        Book book = Book.Register(ValidBookId, ValidIsbn);

        // Act
        Book result = book.Borrow(ValidBorrowerId);

        // Assert
        result.Should().BeSameAs(book);
    }

    #endregion

    #region Borrow - Invalid Scenarios

    [Fact]
    public void Borrow_WhenAlreadyBorrowed_ThrowsInvalidOperationException()
    {
        // Arrange
        Book book = Book.Register(ValidBookId, ValidIsbn);
        book.Borrow(ValidBorrowerId);

        BorrowerId anotherBorrower = BorrowerId.Create("another-borrower");

        // Act
        Action act = () => book.Borrow(anotherBorrower);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already borrowed*");
    }

    [Fact]
    public void Borrow_WhenAlreadyBorrowedBySamePerson_ThrowsInvalidOperationException()
    {
        // Arrange
        Book book = Book.Register(ValidBookId, ValidIsbn);
        book.Borrow(ValidBorrowerId);

        // Act - Try to borrow again by same person
        Action act = () => book.Borrow(ValidBorrowerId);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Return - Valid Scenarios

    [Fact]
    public void Return_WhenBorrowed_ClearsBorrowerIdAndBorrowedOn()
    {
        // Arrange
        Book book = Book.Register(ValidBookId, ValidIsbn);
        book.Borrow(ValidBorrowerId);

        // Act
        book.Return();

        // Assert
        book.BorrowerId.Should().BeNull();
        book.BorrowedOn.Should().BeNull();
        book.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Return_ReturnsSameInstance()
    {
        // Arrange
        Book book = Book.Register(ValidBookId, ValidIsbn);
        book.Borrow(ValidBorrowerId);

        // Act
        Book result = book.Return();

        // Assert
        result.Should().BeSameAs(book);
    }

    #endregion

    #region Return - Invalid Scenarios

    [Fact]
    public void Return_WhenNotBorrowed_ThrowsInvalidOperationException()
    {
        // Arrange
        Book book = Book.Register(ValidBookId, ValidIsbn);

        // Act
        Action act = () => book.Return();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not currently borrowed*");
    }

    [Fact]
    public void Return_WhenAlreadyReturned_ThrowsInvalidOperationException()
    {
        // Arrange
        Book book = Book.Register(ValidBookId, ValidIsbn);
        book.Borrow(ValidBorrowerId);
        book.Return();

        // Act - Try to return again
        Action act = () => book.Return();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Borrow and Return Lifecycle

    [Fact]
    public void Book_CanBeBorrowedAndReturnedMultipleTimes()
    {
        // Arrange
        Book book = Book.Register(ValidBookId, ValidIsbn);
        BorrowerId borrower1 = BorrowerId.Create("borrower-1");
        BorrowerId borrower2 = BorrowerId.Create("borrower-2");

        // Act & Assert - First borrow/return cycle
        book.Borrow(borrower1);
        book.IsAvailable.Should().BeFalse();
        book.BorrowerId.Should().Be(borrower1);

        book.Return();
        book.IsAvailable.Should().BeTrue();

        // Act & Assert - Second borrow/return cycle with different borrower
        book.Borrow(borrower2);
        book.IsAvailable.Should().BeFalse();
        book.BorrowerId.Should().Be(borrower2);

        book.Return();
        book.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Borrow_And_Return_CanBeChained()
    {
        // Arrange
        Book book = Book.Register(ValidBookId, ValidIsbn);

        // Act
        book.Borrow(ValidBorrowerId).Return();

        // Assert
        book.IsAvailable.Should().BeTrue();
    }

    #endregion

    #region IsAvailable Property

    [Fact]
    public void IsAvailable_WhenBorrowerIdIsNull_ReturnsTrue()
    {
        // Arrange
        Book book = Book.Register(ValidBookId, ValidIsbn);

        // Act & Assert
        book.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void IsAvailable_WhenBorrowerIdIsSet_ReturnsFalse()
    {
        // Arrange
        Book book = Book.Register(ValidBookId, ValidIsbn);
        book.Borrow(ValidBorrowerId);

        // Act & Assert
        book.IsAvailable.Should().BeFalse();
    }

    #endregion

    #region Multiple Books with Same ISBN

    [Fact]
    public void Register_MultipleBooksWithSameIsbn_CreatesDistinctBooks()
    {
        // Arrange
        Isbn sharedIsbn = ValidIsbn;
        BookId id1 = BookId.Create("book-copy-1");
        BookId id2 = BookId.Create("book-copy-2");

        // Act
        Book book1 = Book.Register(id1, sharedIsbn);
        Book book2 = Book.Register(id2, sharedIsbn);

        // Assert
        book1.Id.Should().NotBe(book2.Id);
        book1.Isbn.Should().Be(book2.Isbn);
    }

    [Fact]
    public void Borrow_OneCopyDoesNotAffectOtherCopy()
    {
        // Arrange
        Isbn sharedIsbn = ValidIsbn;
        Book book1 = Book.Register(BookId.Create("copy-1"), sharedIsbn);
        Book book2 = Book.Register(BookId.Create("copy-2"), sharedIsbn);

        // Act
        book1.Borrow(ValidBorrowerId);

        // Assert
        book1.IsAvailable.Should().BeFalse();
        book2.IsAvailable.Should().BeTrue();
    }

    #endregion
}

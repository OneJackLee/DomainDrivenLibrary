using DomainDrivenLibrary.Books.Identifier;
using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.CatalogEntries.ValueObjects;
using DomainDrivenLibrary.Entities;

namespace DomainDrivenLibrary.Books;

/// <summary>
///     Represents a physical book copy in the library that can be borrowed.
/// </summary>
public sealed class Book : EntityBase<BookId>
{
    private Book()
    {
        // Reserved for ORM.
    }

    private Book(BookId id, Isbn isbn) : base(id)
    {
        Isbn = isbn;
    }

    /// <summary>
    ///     The ISBN that links this book to its catalog entry.
    /// </summary>
    public Isbn Isbn { get; private set; } = null!;

    /// <summary>
    ///     The ID of the borrower who currently has this book, or null if available.
    /// </summary>
    public BorrowerId? BorrowerId { get; private set; }

    /// <summary>
    ///     The date and time when the book was borrowed, or null if available.
    /// </summary>
    public DateTime? BorrowedOn { get; private set; }

    /// <summary>
    ///     Indicates whether the book is available for borrowing.
    /// </summary>
    public bool IsAvailable => BorrowerId is null;

    /// <summary>
    ///     Registers a new physical book copy in the library.
    /// </summary>
    /// <param name="id">The unique identifier for this book copy.</param>
    /// <param name="isbn">The ISBN linking to the catalog entry.</param>
    /// <returns>A new <see cref="Book" /> instance in available state.</returns>
    public static Book Register(BookId id, Isbn isbn)
    {
        return new Book(id, isbn);
    }

    /// <summary>
    ///     Borrows this book to a library member.
    /// </summary>
    /// <param name="borrowerId">The ID of the borrower checking out this book.</param>
    /// <param name="borrowedOn">The date/time of borrowing. Defaults to UTC now if not specified.</param>
    /// <returns>This <see cref="Book" /> instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the book is already borrowed.</exception>
    public Book Borrow(
        BorrowerId borrowerId,
        DateTime? borrowedOn = null)
    {
        // Enforce invariant: a book can only be borrowed by one member at a time
        if (!IsAvailable)
        {
            throw new InvalidOperationException(
                $"Book is already borrowed since {BorrowedOn:yyyy-MM-dd}.");
        }

        BorrowerId = borrowerId;
        BorrowedOn = borrowedOn ?? DateTime.UtcNow;

        return this;
    }

    /// <summary>
    ///     Returns this book to the library, making it available for borrowing again.
    /// </summary>
    /// <returns>This <see cref="Book" /> instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the book is not currently borrowed.</exception>
    public Book Return()
    {
        // Enforce invariant: cannot return a book that is not borrowed
        if (IsAvailable)
        {
            throw new InvalidOperationException(
                "Book is not currently borrowed and cannot be returned.");
        }

        BorrowerId = null;
        BorrowedOn = null;

        return this;
    }
}
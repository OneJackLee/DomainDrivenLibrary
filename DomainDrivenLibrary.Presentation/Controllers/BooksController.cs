using DomainDrivenLibrary.Books.BorrowBook;
using DomainDrivenLibrary.Books.GetAllBooks;
using DomainDrivenLibrary.Books.RegisterBook;
using DomainDrivenLibrary.Books.ReturnBook;
using DomainDrivenLibrary.Contracts.Requests;
using DomainDrivenLibrary.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DomainDrivenLibrary.Controllers;

/// <summary>
///     API controller for managing books in the library.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class BooksController(
    RegisterBookCommandHandler registerBookHandler,
    GetAllBooksQueryHandler getAllBooksHandler,
    BorrowBookCommandHandler borrowBookHandler,
    ReturnBookCommandHandler returnBookHandler)
    : ControllerBase
{
    /// <summary>
    ///     Registers a new book copy in the library.
    /// </summary>
    /// <param name="request">The book registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly registered book.</returns>
    /// <response code="201">Book successfully registered.</response>
    /// <response code="400">Invalid request (e.g., invalid ISBN format).</response>
    /// <response code="409">ISBN exists with different title/author.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterBook(
        [FromBody] RegisterBookRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterBookCommand(request.Isbn, request.Title, request.Author);
        var result = await registerBookHandler.HandleAsync(command, cancellationToken);
        var response = BookResponse.FromDto(result);

        return CreatedAtAction(nameof(GetAllBooks), response);
    }

    /// <summary>
    ///     Gets all books in the library.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all books with their details.</returns>
    /// <response code="200">Returns the list of books.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBooks(CancellationToken cancellationToken)
    {
        var query = new GetAllBooksQuery();
        var result = await getAllBooksHandler.HandleAsync(query, cancellationToken);
        var response = result.Select(BookResponse.FromDto);

        return Ok(response);
    }

    /// <summary>
    ///     Borrows a book by a library member.
    /// </summary>
    /// <param name="bookId">The ID of the book to borrow.</param>
    /// <param name="request">The borrower details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated book details.</returns>
    /// <response code="200">Book successfully borrowed.</response>
    /// <response code="400">Invalid request (e.g., book already borrowed).</response>
    /// <response code="404">Book or borrower not found.</response>
    [HttpPost("{bookId}/borrow")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BorrowBook(
        [FromRoute] string bookId,
        [FromBody] BorrowBookRequest request,
        CancellationToken cancellationToken)
    {
        var command = new BorrowBookCommand(bookId, request.BorrowerId);
        var result = await borrowBookHandler.HandleAsync(command, cancellationToken);
        var response = BookResponse.FromDto(result);

        return Ok(response);
    }

    /// <summary>
    ///     Returns a borrowed book to the library.
    /// </summary>
    /// <param name="bookId">The ID of the book to return.</param>
    /// <param name="request">The borrower details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated book details.</returns>
    /// <response code="200">Book successfully returned.</response>
    /// <response code="400">Invalid request (e.g., book not borrowed, borrower mismatch).</response>
    /// <response code="404">Book not found.</response>
    [HttpPost("{bookId}/return")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReturnBook(
        [FromRoute] string bookId,
        [FromBody] ReturnBookRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReturnBookCommand(bookId, request.BorrowerId);
        var result = await returnBookHandler.HandleAsync(command, cancellationToken);
        var response = BookResponse.FromDto(result);

        return Ok(response);
    }
}

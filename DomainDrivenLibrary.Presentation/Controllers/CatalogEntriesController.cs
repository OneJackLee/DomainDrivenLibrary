using DomainDrivenLibrary.CatalogEntries.GetCatalogEntryByIsbn;
using DomainDrivenLibrary.CatalogEntries.UpdateCatalogEntry;
using DomainDrivenLibrary.Contracts.Requests;
using DomainDrivenLibrary.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DomainDrivenLibrary.Controllers;

/// <summary>
///     API controller for managing catalog entries in the library.
/// </summary>
[ApiController]
[Route("api/catalog-entries")]
public sealed class CatalogEntriesController(
    GetCatalogEntryByIsbnQueryHandler getCatalogEntryByIsbnHandler,
    UpdateCatalogEntryCommandHandler updateCatalogEntryHandler)
    : ControllerBase
{
    /// <summary>
    ///     Gets a catalog entry by its ISBN.
    /// </summary>
    /// <param name="isbn">The ISBN of the catalog entry.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The catalog entry details.</returns>
    /// <response code="200">Returns the catalog entry.</response>
    /// <response code="400">Invalid ISBN format.</response>
    /// <response code="404">Catalog entry not found.</response>
    [HttpGet("{isbn}")]
    [ProducesResponseType(typeof(CatalogEntryDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIsbn(
        [FromRoute] string isbn,
        CancellationToken cancellationToken)
    {
        var query = new GetCatalogEntryByIsbnQuery(isbn);
        var result = await getCatalogEntryByIsbnHandler.HandleAsync(query, cancellationToken);
        var response = CatalogEntryDetailsResponse.FromDto(result);

        return Ok(response);
    }

    /// <summary>
    ///     Updates a catalog entry's title and author.
    /// </summary>
    /// <param name="isbn">The ISBN of the catalog entry to update.</param>
    /// <param name="request">The new title and author.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated catalog entry.</returns>
    /// <response code="200">Catalog entry successfully updated.</response>
    /// <response code="400">Invalid request (e.g., invalid ISBN format, empty title/author).</response>
    /// <response code="404">Catalog entry not found.</response>
    [HttpPut("{isbn}")]
    [ProducesResponseType(typeof(CatalogEntryDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] string isbn,
        [FromBody] UpdateCatalogEntryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCatalogEntryCommand(isbn, request.Title, request.Author);
        var result = await updateCatalogEntryHandler.HandleAsync(command, cancellationToken);
        var response = CatalogEntryDetailsResponse.FromDto(result);

        return Ok(response);
    }
}

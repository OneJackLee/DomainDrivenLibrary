using DomainDrivenLibrary.Borrowers.RegisterBorrower;
using DomainDrivenLibrary.Contracts.Requests;
using DomainDrivenLibrary.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DomainDrivenLibrary.Controllers;

/// <summary>
///     API controller for managing borrowers in the library.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class BorrowersController(RegisterBorrowerCommandHandler registerBorrowerHandler) : ControllerBase
{
    /// <summary>
    ///     Registers a new borrower in the library.
    /// </summary>
    /// <param name="request">The borrower registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly registered borrower.</returns>
    /// <response code="201">Borrower successfully registered.</response>
    /// <response code="400">Invalid request (e.g., invalid email format, empty name).</response>
    /// <response code="409">Email address already registered.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BorrowerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterBorrower(
        [FromBody] RegisterBorrowerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterBorrowerCommand(request.Name, request.Email);
        var result = await registerBorrowerHandler.HandleAsync(command, cancellationToken);
        var response = BorrowerResponse.FromDto(result);

        return CreatedAtAction(nameof(RegisterBorrower), new { id = response.Id }, response);
    }
}

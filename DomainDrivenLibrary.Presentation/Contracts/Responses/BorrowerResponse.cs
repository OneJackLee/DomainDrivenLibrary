using DomainDrivenLibrary.Borrowers.Shared;

namespace DomainDrivenLibrary.Contracts.Responses;

/// <summary>
///     API response representing a borrower.
/// </summary>
/// <param name="Id">The unique identifier of the borrower.</param>
/// <param name="Name">The borrower's name.</param>
/// <param name="Email">The borrower's email address.</param>
public sealed record BorrowerResponse(string Id, string Name, string Email)
{
    /// <summary>
    ///     Maps from an application DTO to API response.
    /// </summary>
    public static BorrowerResponse FromDto(BorrowerDto dto)
    {
        return new BorrowerResponse(dto.Id, dto.Name, dto.Email);
    }
}

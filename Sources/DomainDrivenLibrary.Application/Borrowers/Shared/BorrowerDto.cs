namespace DomainDrivenLibrary.Borrowers.Shared;

/// <summary>
///     Data transfer object representing a borrower.
///     This DTO is the application layer's output contract.
/// </summary>
/// <param name="Id">The unique identifier of the borrower.</param>
/// <param name="Name">The borrower's name.</param>
/// <param name="Email">The borrower's email address.</param>
public sealed record BorrowerDto(string Id, string Name, string Email)
{
    /// <summary>
    ///     Maps a domain Borrower to this DTO.
    /// </summary>
    /// <param name="borrower">The borrower aggregate from the domain.</param>
    /// <returns>A new <see cref="BorrowerDto" /> instance.</returns>
    public static BorrowerDto FromDomain(Borrower borrower)
    {
        return new BorrowerDto(
            borrower.Id.Value,
            borrower.Name,
            borrower.EmailAddress.Value);
    }
}

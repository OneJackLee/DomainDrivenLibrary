using DomainDrivenLibrary.Borrowers.Shared;
using DomainDrivenLibrary.Dependencies;

namespace DomainDrivenLibrary.Borrowers.GetAllBorrowers;

/// <summary>
///     Handles the query to retrieve all borrowers.
/// </summary>
public sealed class GetAllBorrowersQueryHandler(IBorrowerRepository borrowerRepository)
    : IScopedDependency
{
    /// <summary>
    ///     Executes the query to retrieve all borrowers.
    /// </summary>
    /// <param name="query">The query (no parameters).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of borrower details.</returns>
    public async Task<IReadOnlyList<BorrowerDto>> HandleAsync(
        GetAllBorrowersQuery query,
        CancellationToken cancellationToken = default)
    {
        var borrowers = await borrowerRepository.GetAllAsync(cancellationToken);

        return borrowers
            .Select(BorrowerDto.FromDomain)
            .ToList()
            .AsReadOnly();
    }
}

using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.Borrowers.ValueObjects;

namespace DomainDrivenLibrary.Borrowers;

/// <summary>
///     Repository interface for <see cref="Borrower" /> aggregate persistence.
/// </summary>
public interface IBorrowerRepository
{
    /// <summary>
    ///     Gets a borrower by their unique identifier.
    /// </summary>
    /// <param name="id">The borrower identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The borrower if found; otherwise, null.</returns>
    Task<Borrower?> GetByIdAsync(BorrowerId id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks if a borrower with the specified identifier exists.
    /// </summary>
    /// <param name="id">The borrower identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the borrower exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(BorrowerId id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks if a borrower with the specified email address already exists.
    /// </summary>
    /// <param name="emailAddress">The email address to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a borrower with this email exists; otherwise, false.</returns>
    Task<bool> ExistsByEmailAsync(EmailAddress emailAddress, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a new borrower to the repository.
    /// </summary>
    /// <param name="borrower">The borrower to add.</param>
    void Add(Borrower borrower);
}

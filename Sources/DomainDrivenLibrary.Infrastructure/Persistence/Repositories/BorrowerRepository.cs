using DomainDrivenLibrary.Borrowers;
using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivenLibrary.Persistence.Repositories;

/// <summary>
///     EF Core implementation of <see cref="IBorrowerRepository" />.
/// </summary>
internal sealed class BorrowerRepository(AppDbContext dbContext) : IBorrowerRepository, IScopedDependency
{
    /// <inheritdoc />
    public async Task<Borrower?> GetByIdAsync(BorrowerId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Borrowers
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(BorrowerId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Borrowers
            .AnyAsync(b => b.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public void Add(Borrower borrower) => dbContext.Borrowers.Add(borrower);
}
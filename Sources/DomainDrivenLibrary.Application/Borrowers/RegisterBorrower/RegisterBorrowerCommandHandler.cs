using DomainDrivenLibrary.Borrowers.Identifier;
using DomainDrivenLibrary.Borrowers.Shared;
using DomainDrivenLibrary.Borrowers.ValueObjects;
using DomainDrivenLibrary.Data;
using DomainDrivenLibrary.Dependencies;
using DomainDrivenLibrary.Identifier;

namespace DomainDrivenLibrary.Borrowers.RegisterBorrower;

/// <summary>
///     Handles the registration of a new borrower.
/// </summary>
public sealed class RegisterBorrowerCommandHandler(
    IBorrowerRepository borrowerRepository,
    IIdGenerator idGenerator,
    IUnitOfWork unitOfWork)
    : IScopedDependency
{
    /// <summary>
    ///     Executes the command to register a new borrower.
    /// </summary>
    /// <param name="command">The registration command containing borrower details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly registered borrower details.</returns>
    /// <exception cref="ArgumentException">Thrown when name is empty or email format is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when email address is already registered.</exception>
    public async Task<BorrowerDto> HandleAsync(
        RegisterBorrowerCommand command,
        CancellationToken cancellationToken = default)
    {
        // Create value objects (validation happens here)
        var emailAddress = EmailAddress.Create(command.Email);

        // Validate email uniqueness - one borrower per email address
        var emailExists = await borrowerRepository.ExistsByEmailAsync(emailAddress, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException(
                $"A borrower with email address '{emailAddress}' is already registered.");
        }

        var borrowerId = BorrowerId.Create(idGenerator.New());

        // Create the borrower aggregate
        var borrower = Borrower.Register(borrowerId, command.Name, emailAddress);

        // Persist
        borrowerRepository.Add(borrower);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BorrowerDto.FromDomain(borrower);
    }
}
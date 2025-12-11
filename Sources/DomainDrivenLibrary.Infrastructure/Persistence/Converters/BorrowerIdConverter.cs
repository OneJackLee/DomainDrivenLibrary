using DomainDrivenLibrary.Borrowers.Identifier;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DomainDrivenLibrary.Persistence.Converters;

/// <summary>
///     Converts <see cref="BorrowerId" /> value objects to and from their database string representation.
/// </summary>
public sealed class BorrowerIdConverter() : ValueConverter<BorrowerId, string>(id => id.Value,
    value => BorrowerId.Create(value));
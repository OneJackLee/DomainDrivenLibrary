using DomainDrivenLibrary.Borrowers.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DomainDrivenLibrary.Persistence.Converters;

/// <summary>
///     Converts <see cref="EmailAddress" /> value objects to and from their database string representation.
/// </summary>
public sealed class EmailAddressConverter() : ValueConverter<EmailAddress, string>(email => email.Value,
    value => EmailAddress.Create(value));
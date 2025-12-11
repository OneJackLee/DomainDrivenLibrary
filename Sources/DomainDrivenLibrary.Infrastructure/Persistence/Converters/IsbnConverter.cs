using DomainDrivenLibrary.CatalogEntries.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DomainDrivenLibrary.Persistence.Converters;

/// <summary>
///     Converts <see cref="Isbn" /> value objects to and from their database string representation.
/// </summary>
public sealed class IsbnConverter() : ValueConverter<Isbn, string>(isbn => isbn.Value,
    value => Isbn.Create(value));
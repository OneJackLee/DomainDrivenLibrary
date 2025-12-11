using DomainDrivenLibrary.Books.Identifier;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DomainDrivenLibrary.Persistence.Converters;

/// <summary>
///     Converts <see cref="BookId" /> value objects to and from their database string representation.
/// </summary>
public sealed class BookIdConverter() : ValueConverter<BookId, string>(id => id.Value,
    value => BookId.Create(value));
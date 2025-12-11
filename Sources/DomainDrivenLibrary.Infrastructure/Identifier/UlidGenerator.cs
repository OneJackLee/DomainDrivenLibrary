using DomainDrivenLibrary.Dependencies;

namespace DomainDrivenLibrary.Identifier;

internal sealed class UlidGenerator : IIdGenerator, IScopedDependency
{
    public string New()
    {
        return Ulid.NewUlid()
            .ToString()
            .ToUpperInvariant();
    }
}
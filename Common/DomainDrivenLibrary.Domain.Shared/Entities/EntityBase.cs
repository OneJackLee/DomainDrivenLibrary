namespace DomainDrivenLibrary.Entities;

public abstract class EntityBase<TKey> : IEntity
    where TKey : notnull
{

    protected EntityBase()
    {
        // Required by EF
    }

    protected EntityBase(TKey id)
    {
        Id = id;
    }
    public TKey Id { get; set; } = default!;
}

public abstract class EntityBase : EntityBase<string>
{
    protected EntityBase()
    {
        // Required by EF
    }

    protected EntityBase(string id) : base(id)
    {
        // Nothing
    }
}
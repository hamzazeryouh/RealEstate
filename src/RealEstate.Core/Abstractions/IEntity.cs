namespace RealEstate.Core.Abstractions;

public interface IEntity
{
    // Basic entity marker interface
}

public interface IEntity<TKey> : IEntity
{
    TKey Id { get; set; }
}

public interface ITenantEntity
{
    string TenantId { get; set; }
}

public interface IAuditableEntity
{
    DateTime CreatedDate { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedDate { get; set; }
    string? UpdatedBy { get; set; }
}

public interface ISoftDeletableEntity
{
    bool IsDeleted { get; set; }
    DateTime? DeletedDate { get; set; }
    string? DeletedBy { get; set; }
}

public interface IVersionableEntity
{
    byte[] RowVersion { get; set; }
}

public interface IAggregateRoot : IEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
    void AddDomainEvent(IDomainEvent domainEvent);
}

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
    string EventType { get; }
}
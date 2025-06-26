using RealEstate.Core.Abstractions;

namespace RealEstate.Properties.Domain.Entities;

public abstract class PropertyDomainEvent : IDomainEvent
{
    protected PropertyDomainEvent(Guid propertyId, string tenantId)
    {
        PropertyId = propertyId;
        TenantId = tenantId;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid PropertyId { get; }
    public string TenantId { get; }
    public DateTime OccurredOn { get; }
    public abstract string EventType { get; }
}

public class PropertyPublishedEvent : PropertyDomainEvent
{
    public PropertyPublishedEvent(Guid propertyId, string tenantId) 
        : base(propertyId, tenantId)
    {
    }

    public override string EventType => "PropertyPublished";
}

public class PropertyUnpublishedEvent : PropertyDomainEvent
{
    public PropertyUnpublishedEvent(Guid propertyId, string tenantId) 
        : base(propertyId, tenantId)
    {
    }

    public override string EventType => "PropertyUnpublished";
}

public class PropertyViewedEvent : PropertyDomainEvent
{
    public PropertyViewedEvent(Guid propertyId, string tenantId, int totalViews) 
        : base(propertyId, tenantId)
    {
        TotalViews = totalViews;
    }

    public int TotalViews { get; }
    public override string EventType => "PropertyViewed";
}

public class PropertyInquiryReceivedEvent : PropertyDomainEvent
{
    public PropertyInquiryReceivedEvent(Guid propertyId, string tenantId, int totalInquiries) 
        : base(propertyId, tenantId)
    {
        TotalInquiries = totalInquiries;
    }

    public int TotalInquiries { get; }
    public override string EventType => "PropertyInquiryReceived";
}

public class PropertyCreatedEvent : PropertyDomainEvent
{
    public PropertyCreatedEvent(Guid propertyId, string tenantId, string title, PropertyType type) 
        : base(propertyId, tenantId)
    {
        Title = title;
        Type = type;
    }

    public string Title { get; }
    public PropertyType Type { get; }
    public override string EventType => "PropertyCreated";
}

public class PropertyUpdatedEvent : PropertyDomainEvent
{
    public PropertyUpdatedEvent(Guid propertyId, string tenantId, Dictionary<string, object> changes) 
        : base(propertyId, tenantId)
    {
        Changes = changes;
    }

    public Dictionary<string, object> Changes { get; }
    public override string EventType => "PropertyUpdated";
}

public class PropertyDeletedEvent : PropertyDomainEvent
{
    public PropertyDeletedEvent(Guid propertyId, string tenantId) 
        : base(propertyId, tenantId)
    {
    }

    public override string EventType => "PropertyDeleted";
} 
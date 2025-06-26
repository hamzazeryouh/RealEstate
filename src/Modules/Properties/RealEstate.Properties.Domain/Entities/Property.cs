using RealEstate.Core.Abstractions;

namespace RealEstate.Properties.Domain.Entities;

public class Property : IAggregateRoot, IEntity<Guid>, ITenantEntity, IAuditableEntity, ISoftDeletableEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    
    // Basic Information
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PropertyType Type { get; set; }
    public PropertyStatus Status { get; set; }
    public ListingType ListingType { get; set; }
    
    // Financial Information
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? RentPrice { get; set; }
    public RentPeriod? RentPeriod { get; set; }
    public decimal? ServiceCharge { get; set; }
    public decimal? SecurityDeposit { get; set; }
    
    // Property Details
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? Area { get; set; }
    public AreaUnit? AreaUnit { get; set; }
    public int? YearBuilt { get; set; }
    public int? FloorNumber { get; set; }
    public int? TotalFloors { get; set; }
    public int? ParkingSpaces { get; set; }
    public bool HasGarden { get; set; }
    public bool HasBalcony { get; set; }
    public bool HasTerrace { get; set; }
    public bool HasSwimmingPool { get; set; }
    public bool HasGym { get; set; }
    public bool HasElevator { get; set; }
    public bool HasAirConditioning { get; set; }
    public bool HasHeating { get; set; }
    public bool IsFurnished { get; set; }
    public bool HasStorage { get; set; }
    
    // Location Information
    public Address Address { get; set; } = new();
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Media and Documents
    public List<PropertyImage> Images { get; set; } = new();
    public List<PropertyDocument> Documents { get; set; } = new();
    public string? VirtualTourUrl { get; set; }
    public string? VideoUrl { get; set; }
    
    // Features and Amenities
    public List<PropertyFeature> Features { get; set; } = new();
    public List<PropertyAmenity> Amenities { get; set; } = new();
    
    // Owner Information
    public Guid OwnerId { get; set; }
    
    // Agent Information
    public Guid? AgentId { get; set; }
    
    // Visibility and Marketing
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ViewCount { get; set; }
    public int InquiryCount { get; set; }
    
    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Slug { get; set; }
    
    // Audit Information
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    // Domain Events
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public object[] GetKeys() => new object[] { Id };
    
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    
    // Business Methods
    public void Publish()
    {
        if (!IsPublished)
        {
            IsPublished = true;
            PublishedAt = DateTime.UtcNow;
            AddDomainEvent(new PropertyPublishedEvent(Id, TenantId));
        }
    }
    
    public void Unpublish()
    {
        if (IsPublished)
        {
            IsPublished = false;
            PublishedAt = null;
            AddDomainEvent(new PropertyUnpublishedEvent(Id, TenantId));
        }
    }
    
    public void IncrementViewCount()
    {
        ViewCount++;
        AddDomainEvent(new PropertyViewedEvent(Id, TenantId, ViewCount));
    }
    
    public void IncrementInquiryCount()
    {
        InquiryCount++;
        AddDomainEvent(new PropertyInquiryReceivedEvent(Id, TenantId, InquiryCount));
    }
}

public enum PropertyType
{
    Apartment,
    House,
    Villa,
    Townhouse,
    Penthouse,
    Studio,
    Duplex,
    Land,
    Commercial,
    Office,
    Retail,
    Warehouse,
    Industrial
}

public enum PropertyStatus
{
    Available,
    Sold,
    Rented,
    UnderContract,
    OffMarket,
    Maintenance
}

public enum ListingType
{
    Sale,
    Rent,
    Both
}

public enum RentPeriod
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}

public enum AreaUnit
{
    SquareFeet,
    SquareMeters,
    Acres,
    Hectares
} 
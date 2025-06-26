using RealEstate.Core.Abstractions;

namespace RealEstate.Properties.Domain.Entities;

public class Property : IEntity, ITenantEntity, IAuditableEntity
{
    public Guid PropertyId { get; set; } = Guid.NewGuid();
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
    
    // Media and Documents (simplified as JSON strings)
    public string? ImageUrls { get; set; } // JSON array of image URLs
    public string? DocumentUrls { get; set; } // JSON array of document URLs
    public string? VirtualTourUrl { get; set; }
    public string? VideoUrl { get; set; }
    
    // Features and Amenities (simplified as JSON strings)
    public string? Features { get; set; } // JSON array of features
    public string? Amenities { get; set; } // JSON array of amenities
    
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
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedDate { get; set; }
    public string? DeletedBy { get; set; }
    
    // Business Methods
    public void Publish()
    {
        if (!IsPublished)
        {
            IsPublished = true;
            PublishedAt = DateTime.UtcNow;
        }
    }
    
    public void Unpublish()
    {
        if (IsPublished)
        {
            IsPublished = false;
            PublishedAt = null;
        }
    }
    
    public void IncrementViewCount()
    {
        ViewCount++;
    }
    
    public void IncrementInquiryCount()
    {
        InquiryCount++;
    }
    
    // Static factory method
    public static Property Create(string title, string description, decimal price, PropertyType type, Address address)
    {
        var property = new Property
        {
            PropertyId = Guid.NewGuid(),
            Title = title,
            Description = description,
            Price = price,
            Type = type,
            Address = address,
            Status = PropertyStatus.Available,
            CreatedDate = DateTime.UtcNow
        };
        
        return property;
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
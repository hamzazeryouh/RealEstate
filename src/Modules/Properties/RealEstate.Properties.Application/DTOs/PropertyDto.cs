using RealEstate.Properties.Domain.Entities;

namespace RealEstate.Properties.Application.DTOs;

public class PropertyDto
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PropertyType Type { get; set; }
    public PropertyStatus Status { get; set; }
    public ListingType ListingType { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? RentPrice { get; set; }
    public RentPeriod? RentPeriod { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? Area { get; set; }
    public AreaUnit? AreaUnit { get; set; }
    public AddressDto Address { get; set; } = new();
    public List<PropertyImageDto> Images { get; set; } = new();
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PropertySummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public PropertyType Type { get; set; }
    public PropertyStatus Status { get; set; }
    public ListingType ListingType { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? Area { get; set; }
    public AreaUnit? AreaUnit { get; set; }
    public string City { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string? StreetNumber { get; set; }
    public string? Unit { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? Province { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Neighborhood { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class PropertyImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsMain { get; set; }
}

public class CreatePropertyRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PropertyType Type { get; set; }
    public ListingType ListingType { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? RentPrice { get; set; }
    public RentPeriod? RentPeriod { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? Area { get; set; }
    public AreaUnit? AreaUnit { get; set; }
    public AddressDto Address { get; set; } = new();
    public Guid OwnerId { get; set; }
    public Guid? AgentId { get; set; }
}

public class UpdatePropertyRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public PropertyType? Type { get; set; }
    public PropertyStatus? Status { get; set; }
    public ListingType? ListingType { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public decimal? RentPrice { get; set; }
    public RentPeriod? RentPeriod { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? Area { get; set; }
    public AreaUnit? AreaUnit { get; set; }
    public AddressDto? Address { get; set; }
    public bool? IsPublished { get; set; }
    public bool? IsFeatured { get; set; }
}

public class PropertySearchRequest
{
    public string? Query { get; set; }
    public PropertyType? Type { get; set; }
    public ListingType? ListingType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }
    public int? MaxBedrooms { get; set; }
    public int? MinBathrooms { get; set; }
    public int? MaxBathrooms { get; set; }
    public decimal? MinArea { get; set; }
    public decimal? MaxArea { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public bool? IsFeatured { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
} 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealEstate.Properties.Domain.Entities;

namespace RealEstate.Properties.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly ILogger<PropertiesController> _logger;

    public PropertiesController(ILogger<PropertiesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all properties (demo endpoint)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetProperties()
    {
        try
        {
            // Return sample data for now
            var sampleProperties = new[]
            {
                new 
                {
                    Id = Guid.NewGuid(),
                    Title = "Beautiful Downtown Apartment",
                    Description = "Modern 2-bedroom apartment in the heart of the city",
                    Price = 350000m,
                    Type = PropertyType.Apartment.ToString(),
                    Status = PropertyStatus.Available.ToString(),
                    Bedrooms = 2,
                    Bathrooms = 2,
                    Area = 1200m
                },
                new 
                {
                    Id = Guid.NewGuid(),
                    Title = "Luxury Villa with Pool",
                    Description = "Stunning 4-bedroom villa with swimming pool and garden",
                    Price = 850000m,
                    Type = PropertyType.Villa.ToString(),
                    Status = PropertyStatus.Available.ToString(),
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Area = 3500m
                }
            };
            
            await Task.Delay(1); // Simulate async operation
            return Ok(sampleProperties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting properties");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific property by ID (demo endpoint)
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> GetProperty(Guid id)
    {
        try
        {
            // Return sample data for now
            var sampleProperty = new 
            {
                Id = id,
                Title = "Sample Property",
                Description = "This is a demo property",
                Price = 500000m,
                Type = PropertyType.House.ToString(),
                Status = PropertyStatus.Available.ToString(),
                Bedrooms = 3,
                Bathrooms = 2,
                Area = 2000m,
                Address = new 
                {
                    Street = "123 Main St",
                    City = "Sample City",
                    State = "Sample State",
                    PostalCode = "12345",
                    Country = "Sample Country"
                }
            };

            await Task.Delay(1); // Simulate async operation
            return Ok(sampleProperty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting property {PropertyId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new property (demo endpoint)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<object>> CreateProperty([FromBody] CreatePropertyRequest request)
    {
        try
        {
            var newProperty = new 
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Type = request.Type,
                Status = PropertyStatus.Available.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await Task.Delay(1); // Simulate async operation
            return CreatedAtAction(nameof(GetProperty), new { id = newProperty.Id }, newProperty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating property");
            return StatusCode(500, "Internal server error");
        }
    }
}

// Simple request model
public class CreatePropertyRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Type { get; set; } = string.Empty;
} 
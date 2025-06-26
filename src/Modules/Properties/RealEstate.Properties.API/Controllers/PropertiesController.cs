using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Properties.Application.DTOs;
using RealEstate.Properties.Application.Services;
using RealEstate.Core.Tenant;

namespace RealEstate.Properties.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<PropertiesController> _logger;

    public PropertiesController(
        IPropertyService propertyService,
        ITenantProvider tenantProvider,
        ILogger<PropertiesController> logger)
    {
        _propertyService = propertyService;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    /// <summary>
    /// Get all properties for the current tenant
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PropertySummaryDto>>> GetProperties(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        try
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant not found");

            var properties = await _propertyService.GetPropertiesAsync(
                tenantId, page, pageSize, sortBy, sortDescending);
            
            return Ok(properties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting properties");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific property by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PropertyDto>> GetProperty(Guid id)
    {
        try
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant not found");

            var property = await _propertyService.GetPropertyByIdAsync(id, tenantId);
            if (property == null)
                return NotFound();

            return Ok(property);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting property {PropertyId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new property
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PropertyDto>> CreateProperty([FromBody] CreatePropertyRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant not found");

            var property = await _propertyService.CreatePropertyAsync(request, tenantId);
            
            return CreatedAtAction(
                nameof(GetProperty), 
                new { id = property.Id }, 
                property);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating property");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing property
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PropertyDto>> UpdateProperty(Guid id, [FromBody] UpdatePropertyRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant not found");

            var property = await _propertyService.UpdatePropertyAsync(id, request, tenantId);
            if (property == null)
                return NotFound();

            return Ok(property);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating property {PropertyId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a property
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteProperty(Guid id)
    {
        try
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant not found");

            var result = await _propertyService.DeletePropertyAsync(id, tenantId);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting property {PropertyId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search properties with advanced filters
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<PropertySummaryDto>>> SearchProperties([FromBody] PropertySearchRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant not found");

            var properties = await _propertyService.SearchPropertiesAsync(request, tenantId);
            
            return Ok(properties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching properties");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Publish a property
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult> PublishProperty(Guid id)
    {
        try
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant not found");

            var result = await _propertyService.PublishPropertyAsync(id, tenantId);
            if (!result)
                return NotFound();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing property {PropertyId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Unpublish a property
    /// </summary>
    [HttpPost("{id:guid}/unpublish")]
    public async Task<ActionResult> UnpublishProperty(Guid id)
    {
        try
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant not found");

            var result = await _propertyService.UnpublishPropertyAsync(id, tenantId);
            if (!result)
                return NotFound();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpublishing property {PropertyId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Mark property as featured
    /// </summary>
    [HttpPost("{id:guid}/feature")]
    public async Task<ActionResult> FeatureProperty(Guid id)
    {
        try
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant not found");

            var result = await _propertyService.FeaturePropertyAsync(id, tenantId);
            if (!result)
                return NotFound();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error featuring property {PropertyId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Remove featured status from property
    /// </summary>
    [HttpPost("{id:guid}/unfeature")]
    public async Task<ActionResult> UnfeatureProperty(Guid id)
    {
        try
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant not found");

            var result = await _propertyService.UnfeaturePropertyAsync(id, tenantId);
            if (!result)
                return NotFound();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unfeaturing property {PropertyId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Increment view count for a property
    /// </summary>
    [HttpPost("{id:guid}/view")]
    [AllowAnonymous]
    public async Task<ActionResult> ViewProperty(Guid id)
    {
        try
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant not found");

            await _propertyService.IncrementViewCountAsync(id, tenantId);
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing view count for property {PropertyId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get featured properties
    /// </summary>
    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PropertySummaryDto>>> GetFeaturedProperties(
        [FromQuery] int count = 10)
    {
        try
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant not found");

            var properties = await _propertyService.GetFeaturedPropertiesAsync(tenantId, count);
            
            return Ok(properties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured properties");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get recent properties
    /// </summary>
    [HttpGet("recent")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PropertySummaryDto>>> GetRecentProperties(
        [FromQuery] int count = 10)
    {
        try
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant not found");

            var properties = await _propertyService.GetRecentPropertiesAsync(tenantId, count);
            
            return Ok(properties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent properties");
            return StatusCode(500, "Internal server error");
        }
    }
} 
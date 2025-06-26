# 👨‍💻 Development Guide

## 📋 Table of Contents
- [Getting Started](#getting-started)
- [Coding Standards](#coding-standards)
- [Architecture Patterns](#architecture-patterns)
- [Module Development](#module-development)
- [Testing Guidelines](#testing-guidelines)
- [Database Migrations](#database-migrations)
- [API Development](#api-development)
- [Security Guidelines](#security-guidelines)
- [Performance Optimization](#performance-optimization)
- [Debugging and Troubleshooting](#debugging-and-troubleshooting)

## 🚀 Getting Started

### Development Environment Setup

#### Prerequisites
```bash
# Required Software
- .NET 8 SDK
- Visual Studio 2022 or VS Code
- SQL Server or Docker
- Redis (or Docker)
- Git
- Docker Desktop (optional but recommended)
```

#### IDE Extensions (VS Code)
```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.vscodeintellicode-csharp",
    "ms-vscode.vscode-json",
    "humao.rest-client",
    "ms-mssql.mssql",
    "bradlc.vscode-tailwindcss"
  ]
}
```

#### Project Setup
```bash
# Clone repository
git clone <repository-url>
cd real-estate-saas

# Restore dependencies
dotnet restore RealEstate.sln

# Set up local database
dotnet ef database update --project src/RealEstate.Infrastructure

# Start development services
docker-compose up -d sqlserver redis azurite

# Run the application
dotnet run --project src/RealEstate.Web.Host
```

### Git Workflow

#### Branch Naming Convention
```
feature/RE-123-property-search-enhancement
bugfix/RE-456-authentication-issue
hotfix/RE-789-critical-security-patch
refactor/RE-101-cleanup-property-service
```

#### Commit Message Format
```
feat(properties): add advanced search filters

- Add price range filter
- Add location radius search
- Add property type multi-select
- Update search API documentation

Closes #123
```

## 📜 Coding Standards

### C# Coding Standards

#### Naming Conventions
```csharp
// Classes, Methods, Properties - PascalCase
public class PropertyService
{
    public string PropertyTitle { get; set; }
    public async Task<Property> GetPropertyAsync(Guid propertyId) { }
}

// Local variables, parameters - camelCase
public void UpdateProperty(Guid propertyId, string title)
{
    var existingProperty = await _repository.GetByIdAsync(propertyId);
    var updatedTitle = title.Trim();
}

// Constants - PascalCase
public const int MaxPropertyImages = 50;

// Private fields - _camelCase
private readonly IPropertyRepository _propertyRepository;
```

#### Code Organization
```csharp
// Class structure order:
public class PropertyService
{
    // 1. Constants
    private const int MaxImageSize = 5000000;
    
    // 2. Private fields
    private readonly IPropertyRepository _repository;
    private readonly ILogger<PropertyService> _logger;
    
    // 3. Constructor
    public PropertyService(IPropertyRepository repository, ILogger<PropertyService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    // 4. Public properties
    public string ServiceName { get; } = "PropertyService";
    
    // 5. Public methods
    public async Task<Property> CreatePropertyAsync(CreatePropertyRequest request)
    {
        // Implementation
    }
    
    // 6. Private methods
    private void ValidatePropertyRequest(CreatePropertyRequest request)
    {
        // Implementation
    }
}
```

#### Documentation Standards
```csharp
/// <summary>
/// Creates a new property with the specified details.
/// </summary>
/// <param name="request">The property creation request containing all property details.</param>
/// <param name="cancellationToken">Token to cancel the operation.</param>
/// <returns>The created property with assigned ID and metadata.</returns>
/// <exception cref="ValidationException">Thrown when request validation fails.</exception>
/// <exception cref="DuplicatePropertyException">Thrown when a property with the same address already exists.</exception>
public async Task<Property> CreatePropertyAsync(CreatePropertyRequest request, CancellationToken cancellationToken = default)
{
    // Implementation
}
```

### Entity Framework Conventions

#### Entity Configuration
```csharp
public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        // Table configuration
        builder.ToTable("Properties");
        builder.HasKey(p => p.PropertyId);
        
        // Property configuration
        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(p => p.Price)
            .HasPrecision(18, 2)
            .IsRequired();
        
        // Value object configuration
        builder.OwnsOne(p => p.Address, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200);
            address.Property(a => a.City).HasMaxLength(100);
            address.Property(a => a.State).HasMaxLength(50);
            address.Property(a => a.ZipCode).HasMaxLength(20);
        });
        
        // Index configuration
        builder.HasIndex(p => new { p.TenantId, p.Status })
            .HasDatabaseName("IX_Properties_TenantId_Status");
            
        // Multi-tenancy
        builder.HasQueryFilter(p => p.TenantId == _tenantProvider.GetCurrentTenant());
    }
}
```

## 🏗️ Architecture Patterns

### CQRS Implementation

#### Command Pattern
```csharp
// Command
public record CreatePropertyCommand(
    string Title,
    string Description,
    decimal Price,
    PropertyType Type,
    Address Address
) : IRequest<CreatePropertyResult>;

// Command Handler
public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, CreatePropertyResult>
{
    private readonly IPropertyRepository _repository;
    private readonly IValidator<CreatePropertyCommand> _validator;
    private readonly IMediator _mediator;

    public async Task<CreatePropertyResult> Handle(CreatePropertyCommand command, CancellationToken cancellationToken)
    {
        // 1. Validate command
        await _validator.ValidateAndThrowAsync(command, cancellationToken);
        
        // 2. Create domain entity
        var property = Property.Create(
            command.Title,
            command.Description,
            command.Price,
            command.Type,
            command.Address
        );
        
        // 3. Save to repository
        await _repository.AddAsync(property, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        
        // 4. Publish domain events
        await _mediator.PublishDomainEventsAsync(property, cancellationToken);
        
        return new CreatePropertyResult(property.PropertyId, property.Title);
    }
}

// Validator
public class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
            
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .LessThan(100_000_000);
            
        RuleFor(x => x.Address)
            .NotNull()
            .SetValidator(new AddressValidator());
    }
}
```

#### Query Pattern
```csharp
// Query
public record GetPropertiesQuery(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    PropertyType? Type = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null
) : IRequest<PagedResult<PropertyDto>>;

// Query Handler
public class GetPropertiesQueryHandler : IRequestHandler<GetPropertiesQuery, PagedResult<PropertyDto>>
{
    private readonly IPropertyReadRepository _repository;
    private readonly IMapper _mapper;

    public async Task<PagedResult<PropertyDto>> Handle(GetPropertiesQuery query, CancellationToken cancellationToken)
    {
        var specification = new PropertyFilterSpecification(
            query.SearchTerm,
            query.Type,
            query.MinPrice,
            query.MaxPrice
        );

        var properties = await _repository.GetPagedAsync(
            specification,
            query.Page,
            query.PageSize,
            cancellationToken
        );

        var propertyDtos = _mapper.Map<List<PropertyDto>>(properties.Items);
        
        return new PagedResult<PropertyDto>(
            propertyDtos,
            properties.TotalCount,
            query.Page,
            query.PageSize
        );
    }
}
```

### Domain Events

#### Event Definition
```csharp
public record PropertyCreatedEvent(
    Guid PropertyId,
    string Title,
    decimal Price,
    Address Address,
    DateTime CreatedAt
) : IDomainEvent;
```

#### Event Handler
```csharp
public class PropertyCreatedEventHandler : INotificationHandler<PropertyCreatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ISearchIndexService _searchService;

    public async Task Handle(PropertyCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Update search index
        await _searchService.IndexPropertyAsync(
            notification.PropertyId,
            cancellationToken
        );

        // Send notifications
        await _notificationService.NotifyPropertyCreatedAsync(
            notification,
            cancellationToken
        );
    }
}
```

### Repository Pattern

#### Repository Interface
```csharp
public interface IPropertyRepository : IRepository<Property>
{
    Task<Property?> GetByIdAsync(Guid propertyId, CancellationToken cancellationToken = default);
    Task<PagedResult<Property>> GetPagedAsync(ISpecification<Property> specification, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Property>> GetByAgentIdAsync(Guid agentId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid propertyId, CancellationToken cancellationToken = default);
}
```

#### Repository Implementation
```csharp
public class PropertyRepository : Repository<Property>, IPropertyRepository
{
    public PropertyRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Property?> GetByIdAsync(Guid propertyId, CancellationToken cancellationToken = default)
    {
        return await Context.Properties
            .Include(p => p.Images)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.PropertyId == propertyId, cancellationToken);
    }

    public async Task<List<Property>> GetByAgentIdAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        return await Context.Properties
            .Where(p => p.AgentId == agentId)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync(cancellationToken);
    }
}
```

## 🧪 Testing Guidelines

### Unit Testing

#### Test Structure
```csharp
[Fact]
public async Task CreateProperty_WithValidData_ShouldReturnCreatedProperty()
{
    // Arrange
    var command = new CreatePropertyCommand(
        Title: "Modern Apartment",
        Description: "Beautiful 2BR apartment",
        Price: 450000m,
        Type: PropertyType.Apartment,
        Address: new Address("123 Main St", "New York", "NY", "10001", "USA")
    );

    var mockRepository = new Mock<IPropertyRepository>();
    var mockValidator = new Mock<IValidator<CreatePropertyCommand>>();
    var mockMediator = new Mock<IMediator>();

    mockValidator.Setup(v => v.ValidateAndThrowAsync(It.IsAny<CreatePropertyCommand>(), default))
              .Returns(Task.CompletedTask);

    var handler = new CreatePropertyCommandHandler(
        mockRepository.Object,
        mockValidator.Object,
        mockMediator.Object
    );

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.PropertyId.Should().NotBeEmpty();
    result.Title.Should().Be(command.Title);

    mockRepository.Verify(r => r.AddAsync(It.IsAny<Property>(), default), Times.Once);
    mockRepository.Verify(r => r.SaveChangesAsync(default), Times.Once);
}
```

### Integration Testing

#### Test Base Class
```csharp
public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;

    protected IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Replace database with in-memory
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));

                // Replace external services with mocks
                services.RemoveAll(typeof(IEmailService));
                services.AddSingleton<IEmailService, MockEmailService>();
            });
        });

        Client = Factory.CreateClient();
        Scope = Factory.Services.CreateScope();
    }

    protected async Task<T> GetService<T>() where T : notnull
    {
        return Scope.ServiceProvider.GetRequiredService<T>();
    }
}
```

#### API Integration Test
```csharp
public class PropertiesControllerTests : IntegrationTestBase
{
    public PropertiesControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetProperties_ShouldReturnPagedResult()
    {
        // Arrange
        await SeedTestDataAsync();
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync("/api/properties?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<PropertyDto>>(content, JsonOptions);
        
        result.Should().NotBeNull();
        result.Data.Should().HaveCountGreaterThan(0);
        result.TotalCount.Should().BeGreaterThan(0);
    }

    private async Task SeedTestDataAsync()
    {
        var context = await GetService<ApplicationDbContext>();
        
        context.Properties.AddRange(
            Property.Create("Test Property 1", "Description 1", 100000m, PropertyType.House, TestAddress),
            Property.Create("Test Property 2", "Description 2", 200000m, PropertyType.Apartment, TestAddress)
        );
        
        await context.SaveChangesAsync();
    }
}
```

## 🗃️ Database Migrations

### Creating Migrations
```bash
# Add new migration
dotnet ef migrations add AddPropertyImages --project src/RealEstate.Infrastructure --startup-project src/RealEstate.Web.Host

# Update database
dotnet ef database update --project src/RealEstate.Infrastructure --startup-project src/RealEstate.Web.Host

# Generate SQL script
dotnet ef migrations script --project src/RealEstate.Infrastructure --startup-project src/RealEstate.Web.Host
```

### Migration Best Practices

#### Safe Migration Pattern
```csharp
public partial class AddPropertyImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Add new nullable column first
        migrationBuilder.AddColumn<string>(
            name: "ThumbnailUrl",
            table: "PropertyMedia",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        // 2. Populate data for existing records
        migrationBuilder.Sql(@"
            UPDATE PropertyMedia 
            SET ThumbnailUrl = REPLACE(Url, '/images/', '/thumbnails/')
            WHERE MediaType = 'Image' AND ThumbnailUrl IS NULL
        ");

        // 3. Make column required if needed (separate migration)
        // migrationBuilder.AlterColumn<string>(
        //     name: "ThumbnailUrl",
        //     table: "PropertyMedia",
        //     type: "nvarchar(500)",
        //     maxLength: 500,
        //     nullable: false,
        //     oldClrType: typeof(string),
        //     oldType: "nvarchar(500)",
        //     oldMaxLength: 500,
        //     oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ThumbnailUrl",
            table: "PropertyMedia");
    }
}
```

## 🌐 API Development

### Controller Standards

#### Base Controller
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public abstract class BaseController : ControllerBase
{
    protected readonly IMediator Mediator;

    protected BaseController(IMediator mediator)
    {
        Mediator = mediator;
    }

    protected ActionResult<T> HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.Error switch
        {
            NotFoundError => NotFound(result.Error.Message),
            ValidationError => BadRequest(result.Error.Message),
            UnauthorizedError => Unauthorized(result.Error.Message),
            _ => StatusCode(500, "An error occurred while processing the request")
        };
    }
}
```

#### Properties Controller
```csharp
public class PropertiesController : BaseController
{
    public PropertiesController(IMediator mediator) : base(mediator) { }

    /// <summary>
    /// Get all properties with pagination and filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PropertyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<PropertyDto>>> GetProperties(
        [FromQuery] GetPropertiesQuery query)
    {
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new property
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreatePropertyResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreatePropertyResult>> CreateProperty(
        [FromBody] CreatePropertyCommand command)
    {
        var result = await Mediator.Send(command);
        
        if (result.IsSuccess)
            return CreatedAtAction(
                nameof(GetProperty), 
                new { id = result.Value.PropertyId }, 
                result.Value
            );
            
        return HandleResult(result);
    }
}
```

### Input Validation

#### DTO Validation
```csharp
public class CreatePropertyRequest
{
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(1, 100_000_000)]
    public decimal Price { get; set; }

    [Required]
    public PropertyType Type { get; set; }

    [Required]
    [ValidAddress]
    public Address Address { get; set; } = new();
}

public class ValidAddressAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not Address address)
            return false;

        return !string.IsNullOrWhiteSpace(address.Street) &&
               !string.IsNullOrWhiteSpace(address.City) &&
               !string.IsNullOrWhiteSpace(address.State) &&
               !string.IsNullOrWhiteSpace(address.Country);
    }
}
```

## 🔒 Security Guidelines

### Authentication & Authorization

#### JWT Configuration
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
```

#### Authorization Policies
```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAgentRole", policy =>
        policy.RequireRole("Agent", "Manager", "Admin"));
    
    options.AddPolicy("RequirePropertyOwnership", policy =>
        policy.AddRequirements(new PropertyOwnershipRequirement()));
    
    options.AddPolicy("RequireTenantAccess", policy =>
        policy.AddRequirements(new TenantAccessRequirement()));
});
```

### Input Sanitization
```csharp
public static class InputSanitizer
{
    public static string SanitizeHtml(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return HttpUtility.HtmlEncode(input.Trim());
    }

    public static string SanitizeSearchTerm(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return string.Empty;

        // Remove SQL injection attempts
        var cleaned = Regex.Replace(searchTerm, @"[';\""\-\-/*]", "");
        
        // Limit length
        return cleaned.Length > 100 ? cleaned.Substring(0, 100) : cleaned;
    }
}
```

## ⚡ Performance Optimization

### Caching Strategy
```csharp
public class CachedPropertyService : IPropertyService
{
    private readonly IPropertyService _propertyService;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(15);

    public async Task<PropertyDto?> GetPropertyAsync(Guid propertyId)
    {
        var cacheKey = $"property:{propertyId}";
        
        if (_cache.TryGetValue(cacheKey, out PropertyDto? cachedProperty))
            return cachedProperty;

        var property = await _propertyService.GetPropertyAsync(propertyId);
        
        if (property != null)
        {
            _cache.Set(cacheKey, property, _cacheDuration);
        }

        return property;
    }
}
```

### Database Query Optimization
```csharp
// Good: Efficient query with specific projections
public async Task<List<PropertySummaryDto>> GetPropertySummariesAsync()
{
    return await _context.Properties
        .Where(p => p.IsPublished)
        .Select(p => new PropertySummaryDto
        {
            PropertyId = p.PropertyId,
            Title = p.Title,
            Price = p.Price,
            City = p.Address.City,
            ImageUrl = p.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault()
        })
        .ToListAsync();
}

// Bad: Loading entire entities when only summary needed
public async Task<List<PropertySummaryDto>> GetPropertySummariesBad()
{
    var properties = await _context.Properties
        .Include(p => p.Images)
        .Where(p => p.IsPublished)
        .ToListAsync();

    return properties.Select(p => new PropertySummaryDto
    {
        PropertyId = p.PropertyId,
        Title = p.Title,
        Price = p.Price,
        // ... mapping
    }).ToList();
}
```

## 🐛 Debugging and Troubleshooting

### Logging Configuration
```csharp
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information",
      "RealEstate": "Debug"
    }
  },
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  }
}
```

### Structured Logging
```csharp
public class PropertyService
{
    private readonly ILogger<PropertyService> _logger;

    public async Task<Property> CreatePropertyAsync(CreatePropertyRequest request)
    {
        using var scope = _logger.BeginScope("Creating property {PropertyTitle}", request.Title);
        
        _logger.LogInformation("Starting property creation for {PropertyTitle} with price {Price:C}", 
            request.Title, request.Price);

        try
        {
            var property = // ... create property
            
            _logger.LogInformation("Successfully created property {PropertyId} with title {PropertyTitle}", 
                property.PropertyId, property.Title);
                
            return property;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create property {PropertyTitle}", request.Title);
            throw;
        }
    }
}
```

### Common Issues and Solutions

#### 1. Multi-tenant Data Leakage
```csharp
// Problem: Queries not filtered by tenant
var properties = await _context.Properties.ToListAsync(); // ❌ Returns all tenants' data

// Solution: Always include tenant filter
var properties = await _context.Properties
    .Where(p => p.TenantId == _tenantProvider.GetCurrentTenant())
    .ToListAsync(); // ✅ Returns only current tenant's data
```

#### 2. N+1 Query Problem
```csharp
// Problem: N+1 queries
var properties = await _context.Properties.ToListAsync();
foreach (var property in properties)
{
    var images = await _context.PropertyMedia
        .Where(m => m.PropertyId == property.PropertyId)
        .ToListAsync(); // ❌ N+1 queries
}

// Solution: Use Include
var properties = await _context.Properties
    .Include(p => p.Images)
    .ToListAsync(); // ✅ Single query with joins
```

---

*This development guide ensures consistent, secure, and maintainable code across the entire real estate platform.* 
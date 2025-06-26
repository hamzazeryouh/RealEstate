# 🧪 Testing Guide

## 📋 Table of Contents
- [Testing Philosophy](#testing-philosophy)
- [Testing Structure](#testing-structure)
- [Unit Testing](#unit-testing)
- [Integration Testing](#integration-testing)
- [API Testing](#api-testing)
- [Performance Testing](#performance-testing)
- [Security Testing](#security-testing)
- [Test Data Management](#test-data-management)
- [Continuous Integration](#continuous-integration)

## 🎯 Testing Philosophy

### Testing Pyramid
```
           /\
          /  \    E2E Tests (Few)
         /____\
        /      \  Integration Tests (Some)
       /________\
      /          \ Unit Tests (Many)
     /__________\
```

### Testing Principles
- **Fast**: Tests should run quickly to provide rapid feedback
- **Independent**: Tests should not depend on each other
- **Repeatable**: Tests should produce consistent results
- **Self-Validating**: Tests should have clear pass/fail criteria
- **Timely**: Tests should be written before or alongside production code

### Testing Strategy
- **70% Unit Tests** - Fast, isolated, test individual components
- **20% Integration Tests** - Test component interactions
- **10% End-to-End Tests** - Test complete user workflows

## 📁 Testing Structure

### Project Organization
```
tests/
├── RealEstate.Properties.Tests/          # Unit tests for Properties module
│   ├── Domain/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   └── Services/
│   ├── Application/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── Validators/
│   └── Infrastructure/
│       ├── Repositories/
│       └── Services/
├── RealEstate.Integration.Tests/         # Integration tests
│   ├── Controllers/
│   ├── Database/
│   └── Services/
├── RealEstate.Performance.Tests/         # Performance tests
└── RealEstate.E2E.Tests/                 # End-to-end tests
```

### Test Naming Convention
```csharp
// Pattern: MethodName_Scenario_ExpectedResult
[Fact]
public void CreateProperty_WithValidData_ShouldReturnCreatedProperty()

[Fact]
public void CreateProperty_WithInvalidPrice_ShouldThrowValidationException()

[Fact]
public void GetProperties_WhenUserHasNoAccess_ShouldReturnEmptyResult()
```

## 🔧 Unit Testing

### Test Setup with AutoFixture and FluentAssertions
```csharp
public class PropertyServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IPropertyRepository> _mockRepository;
    private readonly Mock<IValidator<CreatePropertyCommand>> _mockValidator;
    private readonly Mock<ILogger<PropertyService>> _mockLogger;
    private readonly PropertyService _sut; // System Under Test

    public PropertyServiceTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoMoqCustomization());

        _mockRepository = new Mock<IPropertyRepository>();
        _mockValidator = new Mock<IValidator<CreatePropertyCommand>>();
        _mockLogger = new Mock<ILogger<PropertyService>>();

        _sut = new PropertyService(
            _mockRepository.Object,
            _mockValidator.Object,
            _mockLogger.Object
        );
    }
}
```

### Domain Entity Testing
```csharp
public class PropertyTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProperty()
    {
        // Arrange
        var title = "Modern Apartment";
        var description = "Beautiful 2BR apartment";
        var price = 450000m;
        var type = PropertyType.Apartment;
        var address = new Address("123 Main St", "New York", "NY", "10001", "USA");

        // Act
        var property = Property.Create(title, description, price, type, address);

        // Assert
        property.Should().NotBeNull();
        property.PropertyId.Should().NotBeEmpty();
        property.Title.Should().Be(title);
        property.Description.Should().Be(description);
        property.Price.Should().Be(price);
        property.Type.Should().Be(type);
        property.Address.Should().BeEquivalentTo(address);
        property.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        property.Status.Should().Be(PropertyStatus.Draft);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ShouldThrowArgumentException(string invalidTitle)
    {
        // Arrange
        var description = "Valid description";
        var price = 450000m;
        var type = PropertyType.Apartment;
        var address = new Address("123 Main St", "New York", "NY", "10001", "USA");

        // Act & Assert
        var action = () => Property.Create(invalidTitle, description, price, type, address);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*title*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100000)]
    public void Create_WithInvalidPrice_ShouldThrowArgumentException(decimal invalidPrice)
    {
        // Arrange
        var title = "Valid title";
        var description = "Valid description";
        var type = PropertyType.Apartment;
        var address = new Address("123 Main St", "New York", "NY", "10001", "USA");

        // Act & Assert
        var action = () => Property.Create(title, description, invalidPrice, type, address);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*price*");
    }
}
```

### Command Handler Testing
```csharp
public class CreatePropertyCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IPropertyRepository> _mockRepository;
    private readonly Mock<IValidator<CreatePropertyCommand>> _mockValidator;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ITenantProvider> _mockTenantProvider;
    private readonly CreatePropertyCommandHandler _sut;

    public CreatePropertyCommandHandlerTests()
    {
        _fixture = new Fixture();
        _mockRepository = new Mock<IPropertyRepository>();
        _mockValidator = new Mock<IValidator<CreatePropertyCommand>>();
        _mockMediator = new Mock<IMediator>();
        _mockTenantProvider = new Mock<ITenantProvider>();

        _mockTenantProvider.Setup(x => x.GetCurrentTenant())
            .Returns("test-tenant");

        _sut = new CreatePropertyCommandHandler(
            _mockRepository.Object,
            _mockValidator.Object,
            _mockMediator.Object,
            _mockTenantProvider.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreatePropertyAndReturnResult()
    {
        // Arrange
        var command = _fixture.Create<CreatePropertyCommand>();
        var expectedPropertyId = Guid.NewGuid();

        _mockValidator.Setup(x => x.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Property>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<Property, CancellationToken>((property, _) =>
            {
                // Simulate ID assignment by repository
                property.GetType().GetProperty("PropertyId")?.SetValue(property, expectedPropertyId);
            });

        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PropertyId.Should().Be(expectedPropertyId);
        result.Title.Should().Be(command.Title);

        _mockValidator.Verify(x => x.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Property>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(x => x.PublishDomainEventsAsync(It.IsAny<Property>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ShouldThrowValidationException()
    {
        // Arrange
        var command = _fixture.Create<CreatePropertyCommand>();
        var validationException = new ValidationException("Validation failed");

        _mockValidator.Setup(x => x.ValidateAndThrowAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(validationException);

        // Act & Assert
        var action = async () => await _sut.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<ValidationException>()
            .WithMessage("Validation failed");

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Property>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
```

### Repository Testing with In-Memory Database
```csharp
public class PropertyRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PropertyRepository _repository;

    public PropertyRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new PropertyRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingProperty_ShouldReturnProperty()
    {
        // Arrange
        var property = CreateTestProperty();
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(property.PropertyId);

        // Assert
        result.Should().NotBeNull();
        result!.PropertyId.Should().Be(property.PropertyId);
        result.Title.Should().Be(property.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentProperty_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_WithValidProperty_ShouldPersistProperty()
    {
        // Arrange
        var property = CreateTestProperty();

        // Act
        await _repository.AddAsync(property);
        await _repository.SaveChangesAsync();

        // Assert
        var savedProperty = await _context.Properties.FindAsync(property.PropertyId);
        savedProperty.Should().NotBeNull();
        savedProperty!.Title.Should().Be(property.Title);
    }

    private Property CreateTestProperty()
    {
        return Property.Create(
            "Test Property",
            "Test Description",
            100000m,
            PropertyType.House,
            new Address("123 Test St", "Test City", "TS", "12345", "USA")
        );
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

## 🔗 Integration Testing

### Web Application Factory Setup
```csharp
public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> 
    where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove real database
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add in-memory database
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Replace external services with mocks
            services.Replace(ServiceDescriptor.Singleton<IEmailService, MockEmailService>());
            services.Replace(ServiceDescriptor.Singleton<IFileStorageService, MockFileStorageService>());

            // Override tenant provider for testing
            services.Replace(ServiceDescriptor.Scoped<ITenantProvider, TestTenantProvider>());
        });

        builder.UseEnvironment("Testing");
    }
}
```

### Integration Test Base Class
```csharp
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    protected readonly CustomWebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;
    protected readonly ApplicationDbContext Context;

    protected IntegrationTestBase(CustomWebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = Factory.CreateClient();
        Scope = Factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    protected async Task<string> GetAuthTokenAsync(string role = "Agent")
    {
        var user = await CreateTestUserAsync(role);
        var tokenService = Scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var tokenResult = await tokenService.GenerateTokenAsync(user);
        return tokenResult.AccessToken;
    }

    protected async Task<User> CreateTestUserAsync(string role = "Agent")
    {
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = $"test-{Guid.NewGuid()}@example.com",
            FirstName = "Test",
            LastName = "User",
            TenantId = "test-tenant",
            PasswordHash = "hashed-password",
            IsActive = true,
            IsEmailVerified = true
        };

        Context.Users.Add(user);

        var roleEntity = await Context.Roles.FirstOrDefaultAsync(r => r.Name == role);
        if (roleEntity == null)
        {
            roleEntity = new Role
            {
                RoleId = Guid.NewGuid(),
                Name = role,
                TenantId = "test-tenant",
                IsActive = true
            };
            Context.Roles.Add(roleEntity);
        }

        Context.UserRoles.Add(new UserRole
        {
            UserRoleId = Guid.NewGuid(),
            UserId = user.UserId,
            RoleId = roleEntity.RoleId,
            TenantId = "test-tenant"
        });

        await Context.SaveChangesAsync();
        return user;
    }

    protected async Task SeedTestDataAsync()
    {
        var properties = new[]
        {
            CreateTestProperty("Modern Apartment", 450000m, PropertyType.Apartment),
            CreateTestProperty("Family House", 750000m, PropertyType.House),
            CreateTestProperty("Downtown Condo", 320000m, PropertyType.Condo)
        };

        Context.Properties.AddRange(properties);
        await Context.SaveChangesAsync();
    }

    protected Property CreateTestProperty(string title, decimal price, PropertyType type)
    {
        return Property.Create(
            title,
            $"Description for {title}",
            price,
            type,
            new Address("123 Test St", "Test City", "TS", "12345", "USA")
        );
    }

    public void Dispose()
    {
        Scope.Dispose();
        Client.Dispose();
    }
}
```

### Controller Integration Tests
```csharp
public class PropertiesControllerTests : IntegrationTestBase
{
    public PropertiesControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetProperties_WithAuthenticatedUser_ShouldReturnPagedResult()
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
        var result = JsonSerializer.Deserialize<PagedResult<PropertyDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task CreateProperty_WithValidData_ShouldReturnCreatedProperty()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreatePropertyRequest
        {
            Title = "New Test Property",
            Description = "A beautiful test property",
            Price = 500000m,
            Type = PropertyType.House,
            Address = new AddressDto
            {
                Street = "456 New St",
                City = "New City",
                State = "NC",
                ZipCode = "54321",
                Country = "USA"
            }
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/properties", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CreatePropertyResult>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result!.PropertyId.Should().NotBeEmpty();
        result.Title.Should().Be(request.Title);

        // Verify property was actually created in database
        var createdProperty = await Context.Properties.FindAsync(result.PropertyId);
        createdProperty.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProperties_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/properties");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

## 🌐 API Testing

### API Test Collection with xUnit
```csharp
[Collection("API Tests")]
public class PropertyApiTests : IntegrationTestBase
{
    public PropertyApiTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Theory]
    [InlineData("?page=1&pageSize=5", 5)]
    [InlineData("?page=2&pageSize=3", 3)]
    [InlineData("?search=apartment", 1)]
    [InlineData("?type=House", 1)]
    [InlineData("?minPrice=400000", 2)]
    public async Task GetProperties_WithDifferentFilters_ShouldReturnFilteredResults(
        string queryString, int expectedCount)
    {
        // Arrange
        await SeedTestDataAsync();
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync($"/api/properties{queryString}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<PropertyDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result!.Data.Should().HaveCount(expectedCount);
    }
}
```

## ⚡ Performance Testing

### Load Testing with NBomber
```csharp
public class PropertyPerformanceTests
{
    [Fact]
    public void PropertyAPI_LoadTest_ShouldHandleConcurrentRequests()
    {
        var scenario = Scenario.Create("get_properties", async context =>
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", GetTestToken());

            var response = await client.GetAsync("https://localhost:5001/api/properties");
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(2)),
            Simulation.KeepConstant(copies: 5, during: TimeSpan.FromMinutes(1))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }

    [Fact]
    public void Database_PerformanceTest_ShouldExecuteQueriesEfficiently()
    {
        var scenario = Scenario.Create("property_search", async context =>
        {
            using var scope = GetTestServiceScope();
            var repository = scope.ServiceProvider.GetRequiredService<IPropertyRepository>();

            var specification = new PropertyFilterSpecification(
                searchTerm: "apartment",
                minPrice: 100000m,
                maxPrice: 1000000m
            );

            var stopwatch = Stopwatch.StartNew();
            var result = await repository.GetPagedAsync(specification, 1, 20);
            stopwatch.Stop();

            // Assert query executes within acceptable time
            return stopwatch.ElapsedMilliseconds < 100 ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 50, during: TimeSpan.FromSeconds(30))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }
}
```

## 🔒 Security Testing

### Authorization Tests
```csharp
public class SecurityTests : IntegrationTestBase
{
    public SecurityTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetProperty_WithDifferentTenant_ShouldReturnNotFound()
    {
        // Arrange - Create property for tenant A
        var propertyTenantA = CreateTestProperty("Property A", 100000m, PropertyType.House);
        propertyTenantA.TenantId = "tenant-a";
        Context.Properties.Add(propertyTenantA);
        await Context.SaveChangesAsync();

        // Get token for tenant B
        var userTenantB = await CreateTestUserAsync("Agent");
        userTenantB.TenantId = "tenant-b";
        Context.Users.Update(userTenantB);
        await Context.SaveChangesAsync();

        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - Try to access property from tenant A while authenticated as tenant B
        var response = await Client.GetAsync($"/api/properties/{propertyTenantA.PropertyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("Agent", "/api/properties", "GET", HttpStatusCode.OK)]
    [InlineData("Agent", "/api/properties", "POST", HttpStatusCode.Created)]
    [InlineData("User", "/api/properties", "POST", HttpStatusCode.Forbidden)]
    [InlineData("Manager", "/api/users", "GET", HttpStatusCode.OK)]
    [InlineData("Agent", "/api/users", "GET", HttpStatusCode.Forbidden)]
    public async Task API_WithDifferentRoles_ShouldEnforceAuthorization(
        string role, string endpoint, string method, HttpStatusCode expectedStatus)
    {
        // Arrange
        var token = await GetAuthTokenAsync(role);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        HttpResponseMessage response = method switch
        {
            "GET" => await Client.GetAsync(endpoint),
            "POST" => await Client.PostAsync(endpoint, new StringContent("{}", Encoding.UTF8, "application/json")),
            _ => throw new ArgumentException($"Unsupported method: {method}")
        };

        // Assert
        response.StatusCode.Should().Be(expectedStatus);
    }

    [Fact]
    public async Task API_WithMaliciousInput_ShouldRejectRequest()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var maliciousRequest = new CreatePropertyRequest
        {
            Title = "<script>alert('xss')</script>",
            Description = "'; DROP TABLE Properties; --",
            Price = 100000m,
            Type = PropertyType.House,
            Address = new AddressDto
            {
                Street = "123 Test St",
                City = "Test City",
                State = "TS",
                ZipCode = "12345",
                Country = "USA"
            }
        };

        var json = JsonSerializer.Serialize(maliciousRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/properties", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

## 📊 Test Data Management

### Test Data Builders
```csharp
public class PropertyTestDataBuilder
{
    private string _title = "Default Property";
    private string _description = "Default Description";
    private decimal _price = 100000m;
    private PropertyType _type = PropertyType.House;
    private Address _address = new("123 Default St", "Default City", "DC", "12345", "USA");

    public PropertyTestDataBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public PropertyTestDataBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public PropertyTestDataBuilder WithType(PropertyType type)
    {
        _type = type;
        return this;
    }

    public PropertyTestDataBuilder WithAddress(Address address)
    {
        _address = address;
        return this;
    }

    public Property Build()
    {
        return Property.Create(_title, _description, _price, _type, _address);
    }

    public static implicit operator Property(PropertyTestDataBuilder builder)
    {
        return builder.Build();
    }
}

// Usage
var property = new PropertyTestDataBuilder()
    .WithTitle("Luxury Apartment")
    .WithPrice(750000m)
    .WithType(PropertyType.Apartment)
    .Build();
```

### Test Database Seeding
```csharp
public static class TestDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Properties.Any())
            return; // Already seeded

        var properties = new[]
        {
            new PropertyTestDataBuilder()
                .WithTitle("Modern Downtown Apartment")
                .WithPrice(450000m)
                .WithType(PropertyType.Apartment),
                
            new PropertyTestDataBuilder()
                .WithTitle("Suburban Family Home")
                .WithPrice(750000m)
                .WithType(PropertyType.House),
                
            new PropertyTestDataBuilder()
                .WithTitle("Luxury Condo")
                .WithPrice(920000m)
                .WithType(PropertyType.Condo)
        };

        context.Properties.AddRange(properties.Select(p => p.Build()));
        await context.SaveChangesAsync();
    }
}
```

## 🚀 Continuous Integration

### GitHub Actions Test Workflow
```yaml
name: Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          SA_PASSWORD: TestPassword123!
          ACCEPT_EULA: Y
        options: >-
          --health-cmd "/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P TestPassword123! -Q 'SELECT 1'"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
        
    - name: Restore dependencies
      run: dotnet restore RealEstate.sln
      
    - name: Build
      run: dotnet build RealEstate.sln --no-restore
      
    - name: Run Unit Tests
      run: dotnet test tests/RealEstate.Properties.Tests --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
    - name: Run Integration Tests
      run: dotnet test tests/RealEstate.Integration.Tests --no-build --verbosity normal
      env:
        ConnectionStrings__DefaultConnection: "Server=localhost;Database=RealEstateTest;User Id=SA;Password=TestPassword123!;Encrypt=false;"
        
    - name: Upload Coverage Reports
      uses: codecov/codecov-action@v3
      with:
        files: ./coverage.cobertura.xml
        
    - name: Test Report
      uses: dorny/test-reporter@v1
      if: success() || failure()
      with:
        name: Test Results
        path: '**/TestResults.xml'
        reporter: dotnet-trx
```

### Test Configuration
```json
// testsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RealEstateTest;Trusted_Connection=true;"
  },
  "JwtSettings": {
    "SecretKey": "test-secret-key-for-jwt-tokens-minimum-32-characters",
    "Issuer": "RealEstate.Test",
    "Audience": "RealEstate.Test.Client",
    "ExpirationMinutes": 60
  },
  "TenantSettings": {
    "DefaultTenant": "test-tenant"
  }
}
```

---

*This testing guide ensures comprehensive test coverage and maintains code quality throughout the development lifecycle.* 
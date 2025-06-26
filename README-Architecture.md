# Multi-Tenant Real Estate SaaS Application Architecture

A comprehensive, modular real estate management system built with ASP.NET Core (.NET 8+) following Domain-Driven Design (DDD) and multi-tenant SaaS best practices.

## 🏗️ Architecture Overview

This application implements a modular monolith architecture with clear separation of concerns, designed for multi-tenancy from the ground up.

```
RealEstate SaaS Platform
├── Core Infrastructure
│   ├── RealEstate.Core (Shared abstractions & interfaces)
│   ├── RealEstate.Infrastructure (Cross-cutting concerns)
│   └── RealEstate.Shared (Shared utilities & constants)
├── Business Modules
│   ├── Properties Module (Property management)
│   ├── Users Module (User management & authentication)
│   ├── Listings Module (Property listings & search)
│   ├── Contracts Module (Lease & sale contracts)
│   └── Notifications Module (Email, SMS, push notifications)
├── API Layer
│   ├── RealEstate.API.Gateway (API Gateway & routing)
│   └── RealEstate.Web.Host (Main application host)
└── Tests
    ├── Unit Tests (Per module)
    └── Integration Tests (Cross-module & E2E)
```

## 🎯 Key Features

### Multi-Tenancy Support
- **Tenant Isolation**: Complete data separation between tenants
- **Subdomain Routing**: `tenant1.yourdomain.com`, `tenant2.yourdomain.com`
- **Per-Tenant Configuration**: Customizable settings and branding
- **Scalable Architecture**: Database-per-tenant with shared application layer

### Property Management Module
- **Comprehensive Property Data**: All property types (residential, commercial, land)
- **Rich Media Support**: Images, videos, virtual tours, documents
- **Location Services**: Address validation, geocoding, mapping
- **Advanced Search**: Multi-criteria filtering with geo-location
- **SEO Optimization**: Friendly URLs, meta tags, sitemaps

### User Management Module
- **Role-Based Access**: Admin, Agent, Owner, Client roles
- **Multi-Tenant Identity**: Isolated user stores per tenant
- **External Authentication**: Azure AD, Google, Facebook SSO
- **Profile Management**: User preferences and settings

### Listing Management Module
- **Publication Workflow**: Draft → Review → Published states
- **Featured Listings**: Premium placement and promotion
- **Availability Tracking**: Real-time status updates
- **Analytics**: View tracking, inquiry metrics

### Contract Management Module
- **Digital Contracts**: Electronic signature support
- **Template System**: Reusable contract templates
- **Workflow Management**: Approval and review processes
- **Document Storage**: Secure contract archival

### Notification System
- **Multi-Channel**: Email, SMS, push notifications
- **Event-Driven**: Automatic notifications on property events
- **Templates**: Customizable notification templates
- **Preferences**: User-controlled notification settings

## 🏛️ Domain-Driven Design

### Bounded Contexts
Each module represents a bounded context with:
- **Domain Entities**: Rich business objects with behavior
- **Value Objects**: Immutable data containers
- **Domain Events**: Cross-module communication
- **Aggregates**: Consistency boundaries
- **Domain Services**: Complex business logic

### Example: Property Aggregate
```csharp
public class Property : IAggregateRoot
{
    // Rich domain model with business rules
    public void Publish() { /* Business logic */ }
    public void IncrementViewCount() { /* Domain event */ }
    public void UpdatePrice(decimal newPrice) { /* Validation */ }
}
```

## 🔧 Technical Stack

### Backend Technologies
- **Framework**: ASP.NET Core 8.0+
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT + ASP.NET Core Identity
- **Caching**: Redis (distributed) + In-Memory
- **Messaging**: Azure Service Bus (optional)
- **File Storage**: Azure Blob Storage
- **Search**: Elasticsearch (optional)

### Development Tools
- **API Documentation**: Swagger/OpenAPI
- **Testing**: xUnit, Moq, FluentAssertions
- **Mapping**: AutoMapper
- **Validation**: FluentValidation
- **Logging**: Serilog
- **Monitoring**: Application Insights

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server 2019+
- Visual Studio 2022 or VS Code
- Docker (optional)

### Quick Setup

1. **Clone the Repository**
```bash
git clone <repository-url>
cd RealEstate
```

2. **Update Connection Strings**
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=RealEstate;Trusted_Connection=true;",
    "TenantConnection": "Server=.;Database=RealEstate_Tenant_{tenantId};Trusted_Connection=true;"
  }
}
```

3. **Run Database Migrations**
```bash
dotnet ef database update --project src/RealEstate.Infrastructure
```

4. **Start the Application**
```bash
dotnet run --project src/RealEstate.Web.Host
```

5. **Access Swagger UI**
Navigate to `https://localhost:5001/swagger`

## 📁 Project Structure

### Core Layer
```
src/
├── RealEstate.Core/
│   ├── Abstractions/         # Core interfaces (IEntity, IRepository)
│   ├── Tenant/              # Multi-tenancy abstractions
│   └── Events/              # Domain event interfaces
├── RealEstate.Infrastructure/
│   ├── Persistence/         # EF Core configurations
│   ├── MultiTenant/         # Tenant resolution logic
│   ├── Identity/           # Authentication setup
│   └── Services/           # Cross-cutting services
└── RealEstate.Shared/
    ├── Constants/          # Application constants
    ├── Extensions/         # Utility extensions
    └── Helpers/           # Common helpers
```

### Business Modules
```
src/Modules/Properties/
├── RealEstate.Properties.Domain/
│   ├── Entities/           # Property, Address, Features
│   ├── ValueObjects/       # Money, Location, etc.
│   ├── Events/            # Domain events
│   └── Repositories/      # Repository interfaces
├── RealEstate.Properties.Application/
│   ├── Services/          # Application services
│   ├── DTOs/             # Data transfer objects
│   ├── Commands/         # CQRS commands
│   └── Queries/          # CQRS queries
├── RealEstate.Properties.Infrastructure/
│   ├── Persistence/      # EF configurations
│   ├── Repositories/     # Repository implementations
│   └── Services/         # External service integrations
└── RealEstate.Properties.API/
    └── Controllers/      # REST API endpoints
```

## 🔐 Multi-Tenant Security

### Tenant Isolation
```csharp
// Automatic tenant filtering in EF Core
modelBuilder.Entity<Property>()
    .HasQueryFilter(p => p.TenantId == _tenantProvider.GetCurrentTenantId());

// Repository with tenant context
public class PropertyRepository : ITenantRepository<Property>
{
    public async Task<Property> GetByIdAsync(Guid id, string tenantId)
    {
        return await _context.Properties
            .Where(p => p.Id == id && p.TenantId == tenantId)
            .FirstOrDefaultAsync();
    }
}
```

### Tenant Resolution
```csharp
// Subdomain-based tenant resolution
public class TenantResolutionMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;
        var subdomain = host.Split('.')[0];
        var tenant = await _tenantRepository.GetBySubdomainAsync(subdomain);
        
        if (tenant != null)
        {
            context.Items["Tenant"] = tenant;
        }
        
        await _next(context);
    }
}
```

## 📊 API Examples

### Property Management
```http
# Create a property
POST /api/properties
Content-Type: application/json
Authorization: Bearer {token}

{
  "title": "Modern Downtown Apartment",
  "description": "Luxury 2BR/2BA with city views",
  "type": "Apartment",
  "listingType": "Rent",
  "price": 2500.00,
  "bedrooms": 2,
  "bathrooms": 2,
  "area": 1200,
  "address": {
    "street": "123 Main St",
    "city": "New York",
    "state": "NY",
    "postalCode": "10001",
    "country": "USA"
  }
}

# Search properties
POST /api/properties/search
Content-Type: application/json

{
  "type": "Apartment",
  "listingType": "Rent",
  "minPrice": 2000,
  "maxPrice": 3000,
  "minBedrooms": 2,
  "city": "New York",
  "page": 1,
  "pageSize": 20
}
```

### Tenant Management
```http
# Get current tenant info
GET /api/tenants/current
Authorization: Bearer {token}

# Update tenant settings
PUT /api/tenants/current/settings
Content-Type: application/json
Authorization: Bearer {token}

{
  "companyName": "ABC Real Estate",
  "logo": "https://example.com/logo.png",
  "primaryColor": "#007bff",
  "timeZone": "America/New_York"
}
```

## 🧪 Testing Strategy

### Unit Tests
```csharp
[Test]
public async Task CreateProperty_ShouldAddTenantId()
{
    // Arrange
    var tenantProvider = new Mock<ITenantProvider>();
    tenantProvider.Setup(x => x.GetCurrentTenantId()).Returns("tenant1");
    
    var service = new PropertyService(tenantProvider.Object, ...);
    
    // Act
    var property = await service.CreatePropertyAsync(request, "tenant1");
    
    // Assert
    Assert.Equal("tenant1", property.TenantId);
}
```

### Integration Tests
```csharp
[Test]
public async Task GetProperties_ShouldReturnOnlyTenantProperties()
{
    // Arrange
    await SeedTenantDataAsync("tenant1");
    await SeedTenantDataAsync("tenant2");
    
    // Act
    var response = await Client.GetAsync("/api/properties");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var properties = await response.Content.ReadAsAsync<PropertyDto[]>();
    properties.Should().OnlyContain(p => p.TenantId == "tenant1");
}
```

## 🚀 Deployment

### Docker Deployment
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/RealEstate.Web.Host/RealEstate.Web.Host.csproj", "src/RealEstate.Web.Host/"]
RUN dotnet restore "src/RealEstate.Web.Host/RealEstate.Web.Host.csproj"

COPY . .
WORKDIR "/src/src/RealEstate.Web.Host"
RUN dotnet build "RealEstate.Web.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RealEstate.Web.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RealEstate.Web.Host.dll"]
```

### Azure Deployment
```yaml
# azure-pipelines.yml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: DotNetCoreCLI@2
  displayName: 'Restore packages'
  inputs:
    command: 'restore'
    projects: '**/*.csproj'

- task: DotNetCoreCLI@2
  displayName: 'Build application'
  inputs:
    command: 'build'
    arguments: '--configuration Release'

- task: DotNetCoreCLI@2
  displayName: 'Run tests'
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'

- task: DotNetCoreCLI@2
  displayName: 'Publish application'
  inputs:
    command: 'publish'
    publishWebProjects: true
    arguments: '--configuration Release --output $(Build.ArtifactStagingDirectory)'

- task: AzureWebApp@1
  displayName: 'Deploy to Azure App Service'
  inputs:
    azureSubscription: '$(AzureSubscription)'
    appType: 'webApp'
    appName: '$(AppServiceName)'
    package: '$(Build.ArtifactStagingDirectory)/**/*.zip'
```

## 📈 Performance Considerations

### Caching Strategy
- **Property Lists**: Redis cache with 15-minute TTL
- **Tenant Settings**: In-memory cache with change notifications
- **Search Results**: Elasticsearch for complex queries
- **Media Files**: CDN distribution with edge caching

### Database Optimization
- **Indexing**: Comprehensive index strategy for tenant + query patterns
- **Partitioning**: Tenant-based table partitioning for large datasets
- **Read Replicas**: Separate read/write connections for scalability
- **Connection Pooling**: Per-tenant connection pool management

### Monitoring & Observability
- **Application Insights**: Performance and error tracking
- **Custom Metrics**: Tenant-specific usage metrics
- **Health Checks**: Database, cache, and external service monitoring
- **Distributed Tracing**: Cross-module request correlation

## 🔄 Future Enhancements

### Planned Features
- [ ] **Mobile API**: Dedicated mobile app endpoints
- [ ] **Real-time Updates**: SignalR for live notifications
- [ ] **Advanced Search**: Elasticsearch integration
- [ ] **Payment Processing**: Stripe/PayPal integration
- [ ] **Document Management**: Advanced document workflows
- [ ] **Reporting**: Business intelligence dashboards
- [ ] **API Rate Limiting**: Tenant-based throttling
- [ ] **Audit Logging**: Comprehensive audit trails

### Microservices Migration
The modular architecture supports gradual migration to microservices:
1. Extract modules to separate services
2. Implement API contracts between modules
3. Add service discovery and load balancing
4. Implement distributed data patterns

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Built with ❤️ for the real estate industry** 
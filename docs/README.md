# 🏢 Real Estate SaaS Platform Documentation

## 📋 Table of Contents
- [Overview](#overview)
- [System Architecture](#system-architecture)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Business Modules](#business-modules)
- [API Documentation](#api-documentation)
- [Database Design](#database-design)
- [Multi-Tenancy](#multi-tenancy)
- [Development Guidelines](#development-guidelines)
- [Deployment](#deployment)

## 🎯 Overview

The Real Estate SaaS Platform is a comprehensive, multi-tenant application designed for property management, real estate listings, contract management, and client communications. Built using .NET 8 with a modular monolith architecture, it supports multiple real estate agencies through tenant isolation.

### Key Features
- **Multi-Tenant Architecture** - Complete tenant isolation with shared infrastructure
- **Property Management** - Comprehensive property listings with rich metadata
- **User Management** - Role-based access control with agency-specific permissions
- **Contract Management** - Digital contract creation and management
- **Notification System** - Email, SMS, and in-app notifications
- **API Gateway** - Centralized routing and authentication
- **Real-time Updates** - Live property updates and notifications

## 🏗️ System Architecture

The application follows **Clean Architecture** principles with **Domain-Driven Design (DDD)** patterns:

```
┌─────────────────────────────────────────────────┐
│                 Presentation Layer              │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐│
│  │   Web API   │ │ API Gateway │ │   Web Host  ││
│  └─────────────┘ └─────────────┘ └─────────────┘│
└─────────────────────────────────────────────────┘
                        │
┌─────────────────────────────────────────────────┐
│                Application Layer                │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐│
│  │  Commands   │ │   Queries   │ │  Services   ││
│  └─────────────┘ └─────────────┘ └─────────────┘│
└─────────────────────────────────────────────────┘
                        │
┌─────────────────────────────────────────────────┐
│                  Domain Layer                   │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐│
│  │  Entities   │ │Value Objects│ │Domain Events││
│  └─────────────┘ └─────────────┘ └─────────────┘│
└─────────────────────────────────────────────────┘
                        │
┌─────────────────────────────────────────────────┐
│               Infrastructure Layer              │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐│
│  │  Database   │ │   Caching   │ │External APIs││
│  └─────────────┘ └─────────────┘ └─────────────┘│
└─────────────────────────────────────────────────┘
```

### Architectural Patterns
- **CQRS (Command Query Responsibility Segregation)** - Separate read and write operations
- **MediatR** - In-process messaging for decoupling
- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management
- **Domain Events** - Loosely coupled domain interactions

## 💻 Technology Stack

### Backend Technologies
- **.NET 8** - Core framework and runtime
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM for data access
- **SQL Server** - Primary database
- **Redis** - Caching and session storage
- **MediatR** - CQRS and messaging
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Serilog** - Structured logging

### Infrastructure & DevOps
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **Ocelot** - API Gateway
- **Hangfire** - Background job processing
- **Swagger/OpenAPI** - API documentation
- **xUnit** - Unit testing framework
- **Testcontainers** - Integration testing

### External Services
- **Azure Storage** - File and blob storage
- **MailKit** - Email services
- **JWT** - Authentication tokens
- **Prometheus** - Metrics collection
- **Grafana** - Monitoring dashboards

## 📁 Project Structure

```
RealEstate.sln
├── src/
│   ├── RealEstate.Core/              # Core abstractions and interfaces
│   ├── RealEstate.Infrastructure/    # Shared infrastructure services
│   ├── RealEstate.Shared/           # Common utilities and models
│   ├── RealEstate.API.Gateway/      # API Gateway with Ocelot
│   ├── RealEstate.Web.Host/         # Main application host
│   └── Modules/                     # Business modules
│       ├── Properties/              # Property management module
│       ├── Users/                   # User and authentication module
│       ├── Listings/                # Property listings module
│       ├── Contracts/               # Contract management module
│       └── Notifications/           # Notification system module
├── tests/                           # Test projects
├── docs/                           # Documentation
└── docker-compose.yml             # Container orchestration
```

### Module Structure (Example: Properties)
```
RealEstate.Properties.Domain/
├── Entities/           # Domain entities (Property, PropertyMedia)
├── Events/            # Domain events (PropertyCreated, PropertyUpdated)
└── ValueObjects/      # Value objects (Address, Price)

RealEstate.Properties.Application/
├── Commands/          # Write operations (CreateProperty, UpdateProperty)
├── Queries/           # Read operations (GetProperty, SearchProperties)
├── DTOs/             # Data transfer objects
├── Services/         # Application services
└── Validators/       # Input validation rules

RealEstate.Properties.Infrastructure/
├── Data/             # DbContext and entity configurations
├── Repositories/     # Data access implementations
└── Services/         # Infrastructure services

RealEstate.Properties.API/
└── Controllers/      # HTTP endpoints and controllers
```

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- Docker Desktop
- SQL Server (or Docker container)
- Redis (or Docker container)
- Visual Studio 2022 or VS Code

### Installation Steps

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd real-estate-saas
   ```

2. **Start Infrastructure Services**
   ```bash
   docker-compose up -d sqlserver redis azurite
   ```

3. **Restore NuGet Packages**
   ```bash
   dotnet restore RealEstate.sln
   ```

4. **Update Database**
   ```bash
   dotnet ef database update --project src/RealEstate.Infrastructure
   ```

5. **Run the Application**
   ```bash
   dotnet run --project src/RealEstate.Web.Host
   ```

6. **Access Swagger UI**
   ```
   https://localhost:5001/swagger
   ```

### Environment Configuration

Create `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RealEstateDB;Trusted_Connection=true;",
    "Redis": "localhost:6379"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "RealEstate.API",
    "Audience": "RealEstate.Client",
    "ExpirationInMinutes": 60
  },
  "TenantSettings": {
    "DefaultTenant": "demo",
    "ResolutionStrategy": "Subdomain"
  }
}
```

## 📚 Business Modules

### 🏠 Properties Module
**Purpose**: Core property management functionality

**Key Features**:
- Property CRUD operations
- Rich property metadata (40+ fields)
- Image and document management
- Geographic information
- Property valuation tracking
- Search and filtering

**Main Entities**:
- `Property` - Core property entity
- `Address` - Location value object
- `PropertyMedia` - Images and documents

### 👥 Users Module
**Purpose**: User management and authentication

**Key Features**:
- User registration and authentication
- Role-based permissions
- Agency management
- Profile management
- Password policies

**Main Entities**:
- `User` - Application users
- `Role` - User roles and permissions
- `Agency` - Real estate agencies

### 📋 Listings Module
**Purpose**: Property listing and marketing

**Key Features**:
- Public property listings
- Search and filtering
- Featured properties
- Listing analytics
- Marketing campaigns

### 📄 Contracts Module
**Purpose**: Contract and agreement management

**Key Features**:
- Digital contract creation
- Template management
- Electronic signatures
- Contract lifecycle tracking
- Document storage

### 🔔 Notifications Module
**Purpose**: Communication and notification system

**Key Features**:
- Email notifications
- SMS alerts
- In-app notifications
- Notification templates
- Delivery tracking

## 🔐 Multi-Tenancy

The application supports multiple tenancy strategies:

### Tenant Resolution
1. **Subdomain-based**: `tenant1.realestate.com`
2. **Path-based**: `realestate.com/tenant1`
3. **Header-based**: Custom HTTP headers
4. **Claims-based**: JWT token claims

### Data Isolation
- **Database per Tenant** - Complete isolation
- **Schema per Tenant** - Shared database, separate schemas
- **Row-level Security** - Shared tables with tenant filtering

### Implementation
```csharp
public interface ITenantProvider
{
    string GetCurrentTenant();
    TenantInfo GetTenantInfo(string tenantId);
}

public class TenantInfo
{
    public string TenantId { get; set; }
    public string Name { get; set; }
    public string ConnectionString { get; set; }
    public Dictionary<string, object> Settings { get; set; }
}
```

## 📖 Additional Documentation

- [API Reference](./API-Reference.md) - Complete API documentation
- [Database Schema](./Database-Schema.md) - Database design and relationships
- [Development Guide](./Development-Guide.md) - Coding standards and practices
- [Deployment Guide](./Deployment-Guide.md) - Production deployment instructions
- [Security Guide](./Security-Guide.md) - Security best practices
- [Testing Guide](./Testing-Guide.md) - Testing strategies and examples

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Follow coding standards
4. Add comprehensive tests
5. Update documentation
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For support and questions:
- Create an issue on GitHub
- Contact the development team
- Check the documentation

---

*Last updated: [Current Date]*
*Version: 1.0.0* 
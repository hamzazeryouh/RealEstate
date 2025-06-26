# 🗄️ Database Schema Documentation

## 📋 Table of Contents
- [Overview](#overview)
- [Core Tables](#core-tables)
- [Properties Module](#properties-module)
- [Users Module](#users-module)
- [Contracts Module](#contracts-module)
- [Notifications Module](#notifications-module)
- [Relationships](#relationships)
- [Indexes](#indexes)

## 🎯 Overview

The Real Estate SaaS platform uses a **multi-tenant database architecture** with **row-level security** to ensure data isolation between tenants. The schema follows **Domain-Driven Design (DDD)** principles.

### Database Engine
- **Primary**: SQL Server 2022
- **Caching**: Redis
- **File Storage**: Azure Blob Storage

## 🔧 Core Tables

### Tenants
```sql
CREATE TABLE Tenants (
    TenantId NVARCHAR(50) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Domain NVARCHAR(100) UNIQUE,
    ConnectionString NVARCHAR(500),
    Settings NVARCHAR(MAX), -- JSON
    IsActive BIT DEFAULT 1,
    SubscriptionTier NVARCHAR(50),
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 DEFAULT GETUTCDATE()
);
```

## 🏠 Properties Module

### Properties
```sql
CREATE TABLE Properties (
    PropertyId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId NVARCHAR(50) NOT NULL,
    
    -- Basic Information
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    PropertyType NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    
    -- Financial Information
    Price DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(3) DEFAULT 'USD',
    MonthlyRent DECIMAL(10,2),
    
    -- Property Details
    Bedrooms INT,
    Bathrooms DECIMAL(3,1),
    Area DECIMAL(10,2),
    AreaUnit NVARCHAR(10) DEFAULT 'sqft',
    YearBuilt INT,
    ParkingSpaces INT,
    
    -- Location
    AddressStreet NVARCHAR(200),
    AddressCity NVARCHAR(100),
    AddressState NVARCHAR(50),
    AddressZipCode NVARCHAR(20),
    AddressCountry NVARCHAR(50),
    AddressLatitude DECIMAL(10,8),
    AddressLongitude DECIMAL(11,8),
    
    -- Features (JSON Arrays)
    Amenities NVARCHAR(MAX),
    Features NVARCHAR(MAX),
    
    -- Marketing
    IsPublished BIT DEFAULT 0,
    IsFeatured BIT DEFAULT 0,
    ViewCount INT DEFAULT 0,
    
    -- Audit
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0,
    
    -- Indexes
    INDEX IX_Properties_TenantId_Status (TenantId, Status),
    INDEX IX_Properties_Price_Range (Price, TenantId),
    INDEX IX_Properties_Location (AddressLatitude, AddressLongitude),
    
    CONSTRAINT FK_Properties_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId)
);
```

### PropertyMedia
```sql
CREATE TABLE PropertyMedia (
    MediaId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PropertyId UNIQUEIDENTIFIER NOT NULL,
    TenantId NVARCHAR(50) NOT NULL,
    
    MediaType NVARCHAR(20) NOT NULL, -- Image, Video, Document
    FileName NVARCHAR(255) NOT NULL,
    Url NVARCHAR(500) NOT NULL,
    ThumbnailUrl NVARCHAR(500),
    
    Title NVARCHAR(200),
    Caption NVARCHAR(200),
    IsPrimary BIT DEFAULT 0,
    DisplayOrder INT DEFAULT 0,
    
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    
    INDEX IX_PropertyMedia_PropertyId_Type (PropertyId, MediaType),
    
    CONSTRAINT FK_PropertyMedia_Properties FOREIGN KEY (PropertyId) REFERENCES Properties(PropertyId)
);
```

## 👥 Users Module

### Users
```sql
CREATE TABLE Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId NVARCHAR(50) NOT NULL,
    
    -- Authentication
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    
    -- Profile Information
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    
    -- Professional Information
    JobTitle NVARCHAR(100),
    LicenseNumber NVARCHAR(50),
    Bio NVARCHAR(MAX),
    ProfileImageUrl NVARCHAR(500),
    
    -- Account Status
    IsActive BIT DEFAULT 1,
    IsEmailVerified BIT DEFAULT 0,
    LastLoginDate DATETIME2,
    
    -- Audit
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0,
    
    INDEX IX_Users_TenantId_Email (TenantId, Email),
    INDEX IX_Users_Email UNIQUE (Email),
    
    CONSTRAINT FK_Users_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId)
);
```

### Roles
```sql
CREATE TABLE Roles (
    RoleId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId NVARCHAR(50) NOT NULL,
    
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Permissions NVARCHAR(MAX), -- JSON array
    
    IsSystemRole BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Roles_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId)
);
```

### UserRoles
```sql
CREATE TABLE UserRoles (
    UserRoleId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    TenantId NVARCHAR(50) NOT NULL,
    
    AssignedDate DATETIME2 DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    CONSTRAINT UK_UserRoles_User_Role UNIQUE (UserId, RoleId)
);
```

## 📄 Contracts Module

### Contracts
```sql
CREATE TABLE Contracts (
    ContractId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PropertyId UNIQUEIDENTIFIER NOT NULL,
    TenantId NVARCHAR(50) NOT NULL,
    
    ContractNumber NVARCHAR(50),
    ContractType NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    
    -- Financial Terms
    PurchasePrice DECIMAL(18,2),
    DownPayment DECIMAL(18,2),
    
    -- Dates
    ContractDate DATE,
    ClosingDate DATE,
    
    -- Terms
    Terms NVARCHAR(MAX), -- JSON
    Contingencies NVARCHAR(MAX), -- JSON array
    
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Contracts_Properties FOREIGN KEY (PropertyId) REFERENCES Properties(PropertyId)
);
```

## 🔔 Notifications Module

### Notifications
```sql
CREATE TABLE Notifications (
    NotificationId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId NVARCHAR(50) NOT NULL,
    
    RecipientUserId UNIQUEIDENTIFIER,
    Type NVARCHAR(50) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Data NVARCHAR(MAX), -- JSON
    
    Status NVARCHAR(20) DEFAULT 'Pending',
    IsRead BIT DEFAULT 0,
    ReadDate DATETIME2,
    
    Priority NVARCHAR(20) DEFAULT 'Normal',
    
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    
    INDEX IX_Notifications_RecipientUserId_IsRead (RecipientUserId, IsRead),
    
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (RecipientUserId) REFERENCES Users(UserId)
);
```

## 🔗 Key Relationships

### Entity Relationship Overview
```
Tenants (1) -----> (Many) Users
Tenants (1) -----> (Many) Properties
Users (Many) <----> (Many) Roles [UserRoles]
Properties (1) -----> (Many) PropertyMedia
Properties (1) -----> (Many) Contracts
Users (1) -----> (Many) Notifications
```

## 📊 Indexes and Performance

### Primary Indexes
- Clustered primary key indexes on all tables
- Tenant isolation indexes for multi-tenant queries
- Composite indexes for common search patterns

### Spatial Indexes
```sql
-- Location-based property searches
CREATE SPATIAL INDEX IX_Properties_Location ON Properties(LocationPoint);
```

---

*This schema supports multi-tenant SaaS architecture with complete data isolation and optimized performance.* 
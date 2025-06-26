# 🔌 API Reference Documentation

## 📋 Table of Contents
- [Authentication](#authentication)
- [Properties API](#properties-api)
- [Users API](#users-api)
- [Listings API](#listings-api)
- [Contracts API](#contracts-api)
- [Notifications API](#notifications-api)
- [Error Handling](#error-handling)
- [Rate Limiting](#rate-limiting)
- [Webhooks](#webhooks)

## 🔐 Authentication

### Base URL
```
Production: https://api.realestate.com
Development: https://localhost:5001
```

### Authentication Methods

#### JWT Bearer Token
```http
Authorization: Bearer <your-jwt-token>
```

#### API Key (Alternative)
```http
X-API-Key: <your-api-key>
```

#### Tenant Headers
```http
X-Tenant-ID: <tenant-identifier>
```

### Login Endpoint
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "securePassword",
  "tenantId": "agency123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresIn": 3600,
  "user": {
    "id": "user-guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["Agent", "Manager"]
  }
}
```

## 🏠 Properties API

### Get All Properties
```http
GET /api/properties
```

**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `search` (string): Search term
- `type` (string): Property type filter
- `status` (string): Property status filter
- `minPrice` (decimal): Minimum price filter
- `maxPrice` (decimal): Maximum price filter
- `city` (string): City filter
- `sortBy` (string): Sort field (price, createdDate, etc.)
- `sortOrder` (string): asc or desc

**Example Request:**
```http
GET /api/properties?page=1&pageSize=20&type=Apartment&city=NewYork&minPrice=100000&maxPrice=500000&sortBy=price&sortOrder=asc
```

**Response:**
```json
{
  "data": [
    {
      "id": "prop-guid-123",
      "title": "Modern 2BR Apartment in Manhattan",
      "description": "Beautiful apartment with city views...",
      "type": "Apartment",
      "status": "Available",
      "price": 450000,
      "currency": "USD",
      "bedrooms": 2,
      "bathrooms": 2,
      "area": 1200,
      "areaUnit": "sqft",
      "address": {
        "street": "123 Main St",
        "city": "New York",
        "state": "NY",
        "zipCode": "10001",
        "country": "USA",
        "latitude": 40.7128,
        "longitude": -74.0060
      },
      "images": [
        {
          "id": "img-guid-1",
          "url": "https://storage.com/image1.jpg",
          "isPrimary": true,
          "caption": "Living room"
        }
      ],
      "amenities": ["Gym", "Pool", "Parking"],
      "createdDate": "2024-01-15T10:00:00Z",
      "updatedDate": "2024-01-15T10:00:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "totalPages": 25,
    "totalItems": 245,
    "pageSize": 10,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

### Get Property by ID
```http
GET /api/properties/{id}
```

**Response:**
```json
{
  "id": "prop-guid-123",
  "title": "Modern 2BR Apartment in Manhattan",
  "description": "Beautiful apartment with panoramic city views...",
  "type": "Apartment",
  "status": "Available",
  "price": 450000,
  "currency": "USD",
  "pricePerSqft": 375,
  "bedrooms": 2,
  "bathrooms": 2,
  "area": 1200,
  "areaUnit": "sqft",
  "lotSize": 0,
  "yearBuilt": 2020,
  "parkingSpaces": 1,
  "address": {
    "street": "123 Main St",
    "apartment": "4B",
    "city": "New York",
    "state": "NY",
    "zipCode": "10001",
    "country": "USA",
    "latitude": 40.7128,
    "longitude": -74.0060,
    "neighborhood": "Midtown"
  },
  "images": [
    {
      "id": "img-guid-1",
      "url": "https://storage.com/image1.jpg",
      "isPrimary": true,
      "caption": "Living room with city view"
    }
  ],
  "documents": [
    {
      "id": "doc-guid-1",
      "name": "Floor Plan.pdf",
      "url": "https://storage.com/floorplan.pdf",
      "type": "FloorPlan"
    }
  ],
  "amenities": ["Gym", "Pool", "Parking", "Doorman", "Laundry"],
  "features": ["Hardwood Floors", "Stainless Appliances", "City View"],
  "heating": "Central",
  "cooling": "Central Air",
  "utilities": ["Electric", "Gas", "Water"],
  "petPolicy": "Allowed",
  "schoolDistrict": "District 123",
  "isPublished": true,
  "isFeatured": false,
  "viewCount": 156,
  "inquiryCount": 12,
  "agentId": "agent-guid-456",
  "createdDate": "2024-01-15T10:00:00Z",
  "updatedDate": "2024-01-20T15:30:00Z"
}
```

### Create Property
```http
POST /api/properties
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "Modern 2BR Apartment in Manhattan",
  "description": "Beautiful apartment with city views and modern amenities",
  "type": "Apartment",
  "status": "Available",
  "price": 450000,
  "currency": "USD",
  "bedrooms": 2,
  "bathrooms": 2,
  "area": 1200,
  "areaUnit": "sqft",
  "yearBuilt": 2020,
  "parkingSpaces": 1,
  "address": {
    "street": "123 Main St",
    "apartment": "4B",
    "city": "New York",
    "state": "NY",
    "zipCode": "10001",
    "country": "USA"
  },
  "amenities": ["Gym", "Pool", "Parking"],
  "features": ["Hardwood Floors", "City View"],
  "heating": "Central",
  "cooling": "Central Air",
  "petPolicy": "Allowed",
  "isPublished": true
}
```

**Response:**
```json
{
  "id": "prop-guid-new",
  "title": "Modern 2BR Apartment in Manhattan",
  "status": "Available",
  "createdDate": "2024-01-21T09:00:00Z",
  "message": "Property created successfully"
}
```

### Update Property
```http
PUT /api/properties/{id}
Content-Type: application/json
```

### Delete Property
```http
DELETE /api/properties/{id}
```

**Response:**
```json
{
  "success": true,
  "message": "Property deleted successfully"
}
```

### Upload Property Images
```http
POST /api/properties/{id}/images
Content-Type: multipart/form-data
```

**Form Data:**
- `files`: Image files (multiple)
- `captions`: Image captions (optional)
- `isPrimary`: Boolean for primary image

### Search Properties
```http
POST /api/properties/search
Content-Type: application/json
```

**Request Body:**
```json
{
  "searchTerm": "manhattan apartment",
  "filters": {
    "type": ["Apartment", "Condo"],
    "priceRange": {
      "min": 100000,
      "max": 1000000
    },
    "bedrooms": {
      "min": 1,
      "max": 3
    },
    "bathrooms": {
      "min": 1
    },
    "area": {
      "min": 800,
      "max": 2000
    },
    "location": {
      "city": "New York",
      "state": "NY",
      "radius": 10
    },
    "amenities": ["Gym", "Pool"],
    "features": ["Parking"]
  },
  "sortBy": "price",
  "sortOrder": "asc",
  "page": 1,
  "pageSize": 20
}
```

## 👥 Users API

### Get Current User Profile
```http
GET /api/users/profile
```

### Update User Profile
```http
PUT /api/users/profile
Content-Type: application/json
```

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phone": "+1-555-0123",
  "bio": "Experienced real estate agent...",
  "licenseNumber": "RE123456",
  "profileImage": "https://storage.com/profile.jpg"
}
```

### Get Users (Admin/Manager only)
```http
GET /api/users?page=1&pageSize=20&role=Agent
```

### Create User (Admin only)
```http
POST /api/users
Content-Type: application/json
```

**Request Body:**
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com",
  "password": "securePassword123",
  "roles": ["Agent"],
  "agencyId": "agency-guid-123"
}
```

### Change Password
```http
POST /api/users/change-password
Content-Type: application/json
```

**Request Body:**
```json
{
  "currentPassword": "oldPassword",
  "newPassword": "newSecurePassword"
}
```

## 📋 Listings API

### Get Public Listings
```http
GET /api/listings/public
```

**Query Parameters:**
- Same as Properties API plus:
- `featured` (bool): Show only featured listings
- `agentId` (string): Filter by agent

### Get Listing Analytics
```http
GET /api/listings/{id}/analytics
```

**Response:**
```json
{
  "listingId": "listing-guid-123",
  "views": {
    "total": 1245,
    "thisWeek": 89,
    "thisMonth": 356
  },
  "inquiries": {
    "total": 45,
    "thisWeek": 8,
    "thisMonth": 23
  },
  "favorites": 34,
  "shares": 12,
  "contactRequests": 18,
  "virtualTours": 67,
  "performance": {
    "averageTimeOnPage": "00:03:45",
    "bounceRate": 0.32,
    "conversionRate": 0.08
  }
}
```

## 📄 Contracts API

### Get Contracts
```http
GET /api/contracts
```

**Query Parameters:**
- `status` (string): Contract status
- `propertyId` (string): Filter by property
- `clientId` (string): Filter by client

### Create Contract
```http
POST /api/contracts
Content-Type: application/json
```

**Request Body:**
```json
{
  "propertyId": "prop-guid-123",
  "clientId": "client-guid-456",
  "type": "PurchaseAgreement",
  "terms": {
    "purchasePrice": 450000,
    "downPayment": 90000,
    "closingDate": "2024-03-15T00:00:00Z",
    "contingencies": ["Inspection", "Financing"],
    "specialTerms": "Seller to provide warranty"
  },
  "templateId": "template-guid-789"
}
```

### Sign Contract
```http
POST /api/contracts/{id}/sign
Content-Type: application/json
```

**Request Body:**
```json
{
  "signatureType": "Electronic",
  "signature": "base64-signature-data",
  "signerRole": "Buyer",
  "ipAddress": "192.168.1.1",
  "timestamp": "2024-01-21T14:30:00Z"
}
```

## 🔔 Notifications API

### Get Notifications
```http
GET /api/notifications
```

**Response:**
```json
{
  "notifications": [
    {
      "id": "notif-guid-123",
      "type": "PropertyInquiry",
      "title": "New Property Inquiry",
      "message": "Someone is interested in your listing",
      "isRead": false,
      "createdDate": "2024-01-21T10:15:00Z"
    }
  ],
  "unreadCount": 5
}
```

### Mark Notifications as Read
```http
POST /api/notifications/mark-read
Content-Type: application/json
```

**Request Body:**
```json
{
  "notificationIds": ["notif-guid-123", "notif-guid-124"]
}
```

### Send Notification
```http
POST /api/notifications/send
Content-Type: application/json
```

**Request Body:**
```json
{
  "recipientIds": ["user-guid-123"],
  "type": "CustomMessage",
  "title": "Important Update",
  "message": "Your property listing has been approved",
  "channels": ["Email", "InApp"],
  "data": {
    "propertyId": "prop-guid-123",
    "actionUrl": "/properties/prop-guid-123"
  }
}
```

## 🚨 Error Handling

### Standard Error Response
```json
{
  "error": {
    "code": "PROPERTY_NOT_FOUND",
    "message": "The specified property could not be found",
    "details": "Property with ID 'prop-123' does not exist",
    "timestamp": "2024-01-21T10:30:00Z"
  }
}
```

### HTTP Status Codes
- `200 OK` - Success
- `201 Created` - Resource created
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

## 🚦 Rate Limiting

### Rate Limits
- **Public APIs**: 1000 requests per hour
- **Authenticated APIs**: 5000 requests per hour
- **Search APIs**: 100 requests per minute
- **Upload APIs**: 10 requests per minute

### Rate Limit Headers
```http
X-RateLimit-Limit: 5000
X-RateLimit-Remaining: 4999
X-RateLimit-Reset: 1642781400
```

## 🎣 Webhooks

### Property Events
```http
POST /your-webhook-endpoint
Content-Type: application/json
```

**Property Created Event:**
```json
{
  "event": "property.created",
  "timestamp": "2024-01-21T10:00:00Z",
  "data": {
    "propertyId": "prop-guid-123",
    "tenantId": "tenant-123",
    "agentId": "agent-guid-456"
  }
}
```

### Contract Events
```json
{
  "event": "contract.signed",
  "timestamp": "2024-01-21T14:30:00Z",
  "data": {
    "contractId": "contract-guid-789",
    "propertyId": "prop-guid-123",
    "signedBy": "client-guid-456"
  }
}
```

## 🔧 SDK Examples

### C# SDK
```csharp
var client = new RealEstateApiClient("https://api.realestate.com", "your-api-token");

// Get properties
var properties = await client.Properties.GetAllAsync(new PropertySearchRequest
{
    Type = "Apartment",
    City = "New York",
    MinPrice = 100000,
    MaxPrice = 500000
});

// Create property
var newProperty = await client.Properties.CreateAsync(new CreatePropertyRequest
{
    Title = "Modern Apartment",
    Price = 450000,
    Bedrooms = 2,
    Bathrooms = 2
});
```

### JavaScript SDK
```javascript
const client = new RealEstateApiClient({
  baseUrl: 'https://api.realestate.com',
  apiToken: 'your-api-token'
});

// Get properties
const properties = await client.properties.getAll({
  type: 'Apartment',
  city: 'New York',
  minPrice: 100000,
  maxPrice: 500000
});

// Create property
const newProperty = await client.properties.create({
  title: 'Modern Apartment',
  price: 450000,
  bedrooms: 2,
  bathrooms: 2
});
```

---

*For more examples and detailed documentation, visit our [Developer Portal](https://developers.realestate.com)* 
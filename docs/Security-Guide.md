# 🔒 Security Guide

## 📋 Table of Contents
- [Authentication](#authentication)
- [Authorization](#authorization)
- [Multi-Tenant Security](#multi-tenant-security)
- [Data Protection](#data-protection)
- [API Security](#api-security)
- [Input Validation](#input-validation)
- [Security Headers](#security-headers)
- [Audit Logging](#audit-logging)
- [Vulnerability Management](#vulnerability-management)

## 🔐 Authentication

### JWT Authentication Configuration
```csharp
// Startup.cs / Program.cs
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidAudience = configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"])),
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = true,
            RequireSignedTokens = true
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Log authentication failures
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                logger.LogWarning("JWT authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                // Additional token validation
                var userService = context.HttpContext.RequestServices
                    .GetRequiredService<IUserService>();
                return userService.ValidateTokenAsync(context);
            }
        };
    });
```

### Secure Token Generation
```csharp
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserService _userService;

    public async Task<TokenResult> GenerateTokenAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("tenant_id", user.TenantId),
            new("user_version", user.SecurityStamp), // For token invalidation
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, 
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add role claims
        var roles = await _userService.GetUserRolesAsync(user.UserId);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        // Store refresh token securely
        await StoreRefreshTokenAsync(user.UserId, refreshToken);

        return new TokenResult
        {
            AccessToken = tokenString,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtSettings.ExpirationMinutes * 60,
            TokenType = "Bearer"
        };
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
```

### Password Security
```csharp
public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12; // BCrypt work factor
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 128;

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty");

        if (password.Length < MinPasswordLength || password.Length > MaxPasswordLength)
            throw new ArgumentException($"Password must be between {MinPasswordLength} and {MaxPasswordLength} characters");

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            return false; // Invalid hash format
        }
    }

    public bool IsPasswordComplex(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
            return false;

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}
```

## 🛡️ Authorization

### Policy-Based Authorization
```csharp
// Program.cs
services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("RequireAgentRole", policy =>
        policy.RequireRole("Agent", "Manager", "Admin"));

    options.AddPolicy("RequireManagerRole", policy =>
        policy.RequireRole("Manager", "Admin"));

    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));

    // Resource-based policies
    options.AddPolicy("PropertyOwnership", policy =>
        policy.AddRequirements(new PropertyOwnershipRequirement()));

    options.AddPolicy("TenantAccess", policy =>
        policy.AddRequirements(new TenantAccessRequirement()));

    // Permission-based policies
    options.AddPolicy("CanManageProperties", policy =>
        policy.AddRequirements(new PermissionRequirement("properties:manage")));

    options.AddPolicy("CanViewAnalytics", policy =>
        policy.AddRequirements(new PermissionRequirement("analytics:view")));
});

// Register authorization handlers
services.AddScoped<IAuthorizationHandler, PropertyOwnershipHandler>();
services.AddScoped<IAuthorizationHandler, TenantAccessHandler>();
services.AddScoped<IAuthorizationHandler, PermissionHandler>();
```

### Custom Authorization Requirements
```csharp
public class PropertyOwnershipRequirement : IAuthorizationRequirement
{
    public string PropertyIdParameter { get; set; } = "id";
}

public class PropertyOwnershipHandler : AuthorizationHandler<PropertyOwnershipRequirement>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PropertyOwnershipRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return;
        }

        // Get property ID from route
        if (!httpContext.Request.RouteValues.TryGetValue(requirement.PropertyIdParameter, out var propertyIdValue) ||
            !Guid.TryParse(propertyIdValue?.ToString(), out var propertyId))
        {
            context.Fail();
            return;
        }

        // Get current user
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            context.Fail();
            return;
        }

        // Check ownership or admin role
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var property = await _propertyRepository.GetByIdAsync(propertyId);
        if (property != null && property.AgentId == userId)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
```

### Controller Authorization
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "TenantAccess")]
public class PropertiesController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "RequireAgentRole")]
    public async Task<ActionResult<PagedResult<PropertyDto>>> GetProperties()
    {
        // Implementation
    }

    [HttpPost]
    [Authorize(Policy = "CanManageProperties")]
    public async Task<ActionResult<CreatePropertyResult>> CreateProperty()
    {
        // Implementation
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "PropertyOwnership")]
    public async Task<ActionResult> UpdateProperty(Guid id)
    {
        // Implementation
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteProperty(Guid id)
    {
        // Implementation
    }
}
```

## 🏢 Multi-Tenant Security

### Tenant Isolation Middleware
```csharp
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        try
        {
            await tenantProvider.ResolveCurrentTenantAsync(context);
            
            // Validate tenant access
            var currentTenant = tenantProvider.GetCurrentTenant();
            if (string.IsNullOrEmpty(currentTenant))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid tenant");
                return;
            }

            // Check tenant is active
            var tenantInfo = await tenantProvider.GetTenantInfoAsync(currentTenant);
            if (tenantInfo == null || !tenantInfo.IsActive)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Tenant not found or inactive");
                return;
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in tenant resolution");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error");
        }
    }
}
```

### Tenant Data Filter
```csharp
public class TenantQueryFilter : IQueryFilter
{
    private readonly ITenantProvider _tenantProvider;

    public void Apply(ModelBuilder modelBuilder)
    {
        var currentTenant = _tenantProvider.GetCurrentTenant();
        
        // Apply tenant filter to all ITenantEntity implementations
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
                var filter = Expression.Equal(property, Expression.Constant(currentTenant));
                var lambda = Expression.Lambda(filter, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
```

## 🔐 Data Protection

### Encryption Configuration
```csharp
services.AddDataProtection()
    .SetApplicationName("RealEstate")
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90))
    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });
```

### Sensitive Data Encryption
```csharp
public class EncryptionService : IEncryptionService
{
    private readonly IDataProtector _protector;

    public EncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector("SensitiveData");
    }

    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return plaintext;

        return _protector.Protect(plaintext);
    }

    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return ciphertext;

        try
        {
            return _protector.Unprotect(ciphertext);
        }
        catch (CryptographicException)
        {
            // Log decryption failure
            return null;
        }
    }
}

// Usage in entity
public class User
{
    private string _socialSecurityNumber;

    public string SocialSecurityNumber
    {
        get => _encryptionService.Decrypt(_socialSecurityNumber);
        set => _socialSecurityNumber = _encryptionService.Encrypt(value);
    }
}
```

## 🌐 API Security

### Rate Limiting
```csharp
services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 1000,
                Window = TimeSpan.FromHours(1)
            }));

    options.AddPolicy("AuthenticatedUser", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? "anonymous",
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5000,
                Window = TimeSpan.FromHours(1),
                SegmentsPerWindow = 6
            }));

    options.AddPolicy("SearchAPI", httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 100,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                TokensPerPeriod = 20,
                AutoReplenishment = true
            }));
});
```

### CORS Configuration
```csharp
services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", builder =>
    {
        builder
            .WithOrigins("https://app.realestate.com", "https://admin.realestate.com")
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Authorization", "Content-Type", "X-Tenant-ID")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });

    options.AddPolicy("DevelopmentPolicy", builder =>
    {
        builder
            .WithOrigins("http://localhost:3000", "https://localhost:3001")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
```

## ✅ Input Validation

### Request Validation
```csharp
public class CreatePropertyRequestValidator : AbstractValidator<CreatePropertyRequest>
{
    public CreatePropertyRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(5, 200).WithMessage("Title must be between 5 and 200 characters")
            .Must(NotContainMaliciousContent).WithMessage("Title contains invalid characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .Must(NotContainMaliciousContent).WithMessage("Description contains invalid characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThan(100_000_000).WithMessage("Price cannot exceed $100,000,000");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .Must(NotBeDisposableEmail).WithMessage("Disposable email addresses are not allowed");
    }

    private bool NotContainMaliciousContent(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        // Check for common XSS patterns
        var maliciousPatterns = new[]
        {
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
            @"javascript:",
            @"vbscript:",
            @"on\w+\s*=",
            @"expression\s*\("
        };

        return !maliciousPatterns.Any(pattern => 
            Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
    }
}
```

### Input Sanitization
```csharp
public static class InputSanitizer
{
    public static string SanitizeHtml(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Use HtmlSanitizer library for comprehensive cleaning
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedTags.Add("b");
        sanitizer.AllowedTags.Add("i");
        sanitizer.AllowedTags.Add("u");
        sanitizer.AllowedTags.Add("p");
        sanitizer.AllowedTags.Add("br");

        return sanitizer.Sanitize(input);
    }

    public static string SanitizeForDatabase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove null bytes and control characters
        return Regex.Replace(input.Trim(), @"[\x00-\x1F\x7F]", "");
    }

    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "file";

        // Remove invalid file name characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        
        return string.IsNullOrWhiteSpace(sanitized) ? "file" : sanitized;
    }
}
```

## 🔒 Security Headers

### Security Headers Middleware
```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Content Security Policy
        context.Response.Headers.Add("Content-Security-Policy", 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' https://trusted-cdn.com; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' https://fonts.gstatic.com; " +
            "connect-src 'self' https://api.realestate.com; " +
            "frame-ancestors 'none'");

        // Security headers
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        
        // HSTS for HTTPS
        if (context.Request.IsHttps)
        {
            context.Response.Headers.Add("Strict-Transport-Security", 
                "max-age=31536000; includeSubDomains; preload");
        }

        // Remove server information
        context.Response.Headers.Remove("Server");

        await _next(context);
    }
}
```

## 📋 Audit Logging

### Audit Logging Implementation
```csharp
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuditLogger _auditLogger;

    public async Task InvokeAsync(HttpContext context)
    {
        var auditEntry = new AuditEntry
        {
            UserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            TenantId = context.User.FindFirst("tenant_id")?.Value,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers["User-Agent"],
            Path = context.Request.Path,
            Method = context.Request.Method,
            Timestamp = DateTime.UtcNow
        };

        // Capture request body for sensitive operations
        if (IsSensitiveOperation(context.Request.Path, context.Request.Method))
        {
            context.Request.EnableBuffering();
            auditEntry.RequestBody = await ReadRequestBodyAsync(context.Request);
        }

        await _next(context);

        auditEntry.StatusCode = context.Response.StatusCode;
        await _auditLogger.LogAsync(auditEntry);
    }

    private bool IsSensitiveOperation(string path, string method)
    {
        var sensitiveOperations = new[]
        {
            ("/api/auth/login", "POST"),
            ("/api/users", "POST"),
            ("/api/users/password", "PUT"),
            ("/api/properties", "POST"),
            ("/api/properties/", "PUT"),
            ("/api/properties/", "DELETE")
        };

        return sensitiveOperations.Any(op => 
            path.StartsWith(op.Item1, StringComparison.OrdinalIgnoreCase) && 
            method.Equals(op.Item2, StringComparison.OrdinalIgnoreCase));
    }
}
```

## 🛡️ Vulnerability Management

### Security Scanning Pipeline
```yaml
# .github/workflows/security-scan.yml
name: Security Scan

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]
  schedule:
    - cron: '0 2 * * 0'  # Weekly scan

jobs:
  security-scan:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Run OWASP Dependency Check
      uses: dependency-check/Dependency-Check_Action@main
      with:
        project: 'RealEstate'
        path: '.'
        format: 'JSON'
        
    - name: Run CodeQL Analysis
      uses: github/codeql-action/analyze@v2
      with:
        languages: csharp
        
    - name: Run Semgrep Security Scanner
      uses: returntocorp/semgrep-action@v1
      with:
        config: auto
```

### Security Configuration Checklist
```csharp
// Security configuration validation
public class SecurityConfigurationValidator
{
    public void ValidateConfiguration(IConfiguration configuration)
    {
        var issues = new List<string>();

        // JWT settings validation
        var jwtSecret = configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
            issues.Add("JWT secret key must be at least 32 characters");

        // Database connection validation
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (connectionString?.Contains("Password=") == true && !connectionString.Contains("Encrypt=true"))
            issues.Add("Database connection must use encryption");

        // HTTPS validation
        var urls = configuration["Urls"];
        if (!string.IsNullOrEmpty(urls) && !urls.Contains("https://"))
            issues.Add("Application must use HTTPS in production");

        // Cookie settings
        var cookieSecure = configuration.GetValue<bool>("CookieSettings:Secure");
        if (!cookieSecure)
            issues.Add("Cookies must be marked as secure in production");

        if (issues.Any())
            throw new SecurityException($"Security configuration issues: {string.Join(", ", issues)}");
    }
}
```

---

*This security guide provides comprehensive protection for the real estate SaaS platform against common security threats and vulnerabilities.* 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RealEstate.Core.Tenant;
using RealEstate.Infrastructure.MultiTenant;
using RealEstate.Infrastructure.Persistence;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "RealEstate API", 
        Version = "v1",
        Description = "Multi-tenant Real Estate Management System"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Multi-tenancy
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Add Database Context
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var tenantProvider = serviceProvider.GetService<ITenantProvider>();
    var connectionString = tenantProvider?.GetCurrentTenantAsync().Result?.ConnectionString 
                          ?? builder.Configuration.GetConnectionString("DefaultConnection");
    
    options.UseSqlServer(connectionString);
});

// Add Application Services
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();

// Add AutoMapper
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Add Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>();

// Add Memory Cache
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add Multi-tenant middleware
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseCors("DefaultPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapControllers();
app.MapHealthChecks("/health");

// Map module endpoints
app.MapGroup("/api/properties")
   .WithTags("Properties")
   .RequireAuthorization();

app.MapGroup("/api/users")
   .WithTags("Users")
   .RequireAuthorization();

app.MapGroup("/api/listings")
   .WithTags("Listings")
   .RequireAuthorization();

app.MapGroup("/api/contracts")
   .WithTags("Contracts")
   .RequireAuthorization();

app.MapGroup("/api/notifications")
   .WithTags("Notifications")
   .RequireAuthorization();

// Seed database if needed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsDevelopment())
    {
        context.Database.EnsureCreated();
        // Seed development data here
    }
}

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { } 
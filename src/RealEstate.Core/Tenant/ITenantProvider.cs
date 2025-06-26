namespace RealEstate.Core.Tenant;

public interface ITenantProvider
{
    string? GetCurrentTenantId();
    Task<TenantInfo?> GetCurrentTenantAsync();
    Task<TenantInfo?> GetTenantAsync(string tenantId);
    Task<bool> TenantExistsAsync(string tenantId);
}

public class TenantInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string? Subdomain { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public Dictionary<string, string> Settings { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public interface ITenantRepository
{
    Task<TenantInfo?> GetByIdAsync(string tenantId);
    Task<TenantInfo?> GetBySubdomainAsync(string subdomain);
    Task<TenantInfo?> GetByDomainAsync(string domain);
    Task<IEnumerable<TenantInfo>> GetAllActiveAsync();
    Task<TenantInfo> CreateAsync(TenantInfo tenant);
    Task UpdateAsync(TenantInfo tenant);
    Task DeleteAsync(string tenantId);
} 
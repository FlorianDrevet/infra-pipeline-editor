using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;

/// <summary>
/// Represents a CORS (Cross-Origin Resource Sharing) rule on a Storage Account
/// for either the Blob or Table service.
/// </summary>
public class CorsRule : Entity<CorsRuleId>
{
    /// <summary>Gets the parent Storage Account identifier.</summary>
    public AzureResourceId StorageAccountId { get; private set; } = null!;

    /// <summary>Gets the storage service type this rule applies to (Blob or Table).</summary>
    public CorsServiceType ServiceType { get; private set; } = null!;

    /// <summary>Gets the allowed origin domains.</summary>
    public List<string> AllowedOrigins { get; private set; } = [];

    /// <summary>Gets the allowed HTTP methods.</summary>
    public List<string> AllowedMethods { get; private set; } = [];

    /// <summary>Gets the allowed request headers.</summary>
    public List<string> AllowedHeaders { get; private set; } = [];

    /// <summary>Gets the response headers exposed to the client.</summary>
    public List<string> ExposedHeaders { get; private set; } = [];

    /// <summary>Gets the maximum age in seconds that a preflight response can be cached.</summary>
    public int MaxAgeInSeconds { get; private set; }

    private CorsRule(CorsRuleId id) : base(id)
    {
    }

    private CorsRule()
    {
    }

    /// <summary>Updates all properties of this CORS rule.</summary>
    public void Update(
        CorsServiceType serviceType,
        IReadOnlyCollection<string> allowedOrigins,
        IReadOnlyCollection<string> allowedMethods,
        IReadOnlyCollection<string> allowedHeaders,
        IReadOnlyCollection<string> exposedHeaders,
        int maxAgeInSeconds)
    {
        ServiceType = serviceType;
        AllowedOrigins = [.. allowedOrigins];
        AllowedMethods = [.. allowedMethods];
        AllowedHeaders = [.. allowedHeaders];
        ExposedHeaders = [.. exposedHeaders];
        MaxAgeInSeconds = maxAgeInSeconds;
    }

    /// <summary>Creates a new <see cref="CorsRule"/> with a generated identifier.</summary>
    public static CorsRule Create(
        AzureResourceId storageAccountId,
        CorsServiceType serviceType,
        IReadOnlyCollection<string> allowedOrigins,
        IReadOnlyCollection<string> allowedMethods,
        IReadOnlyCollection<string> allowedHeaders,
        IReadOnlyCollection<string> exposedHeaders,
        int maxAgeInSeconds)
    {
        return new CorsRule(CorsRuleId.CreateUnique())
        {
            StorageAccountId = storageAccountId,
            ServiceType = serviceType,
            AllowedOrigins = [.. allowedOrigins],
            AllowedMethods = [.. allowedMethods],
            AllowedHeaders = [.. allowedHeaders],
            ExposedHeaders = [.. exposedHeaders],
            MaxAgeInSeconds = maxAgeInSeconds
        };
    }
}
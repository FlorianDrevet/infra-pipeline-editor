using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;

public class CorsRule : Entity<CorsRuleId>
{
    public AzureResourceId StorageAccountId { get; private set; } = null!;

    public CorsServiceType ServiceType { get; private set; } = null!;

    public List<string> AllowedOrigins { get; private set; } = [];

    public List<string> AllowedMethods { get; private set; } = [];

    public List<string> AllowedHeaders { get; private set; } = [];

    public List<string> ExposedHeaders { get; private set; } = [];

    public int MaxAgeInSeconds { get; private set; }

    private CorsRule(CorsRuleId id) : base(id)
    {
    }

    private CorsRule()
    {
    }

    public void Update(
        CorsServiceType serviceType,
        IReadOnlyList<string> allowedOrigins,
        IReadOnlyList<string> allowedMethods,
        IReadOnlyList<string> allowedHeaders,
        IReadOnlyList<string> exposedHeaders,
        int maxAgeInSeconds)
    {
        ServiceType = serviceType;
        AllowedOrigins = [.. allowedOrigins];
        AllowedMethods = [.. allowedMethods];
        AllowedHeaders = [.. allowedHeaders];
        ExposedHeaders = [.. exposedHeaders];
        MaxAgeInSeconds = maxAgeInSeconds;
    }

    public static CorsRule Create(
        AzureResourceId storageAccountId,
        CorsServiceType serviceType,
        IReadOnlyList<string> allowedOrigins,
        IReadOnlyList<string> allowedMethods,
        IReadOnlyList<string> allowedHeaders,
        IReadOnlyList<string> exposedHeaders,
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
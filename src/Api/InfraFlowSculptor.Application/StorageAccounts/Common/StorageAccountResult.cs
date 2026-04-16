using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.StorageAccounts.Common;

public record BlobContainerResult(
    BlobContainerId Id,
    string Name,
    BlobContainerPublicAccess PublicAccess
);

public record CorsRuleResult(
    IReadOnlyCollection<string> AllowedOrigins,
    IReadOnlyCollection<string> AllowedMethods,
    IReadOnlyCollection<string> AllowedHeaders,
    IReadOnlyCollection<string> ExposedHeaders,
    int MaxAgeInSeconds
);

public record StorageQueueResult(
    StorageQueueId Id,
    string Name
);

public record StorageTableResult(
    StorageTableId Id,
    string Name
);

public record BlobLifecycleRuleResult(
    string RuleName,
    IReadOnlyCollection<string> ContainerNames,
    int TimeToLiveInDays
);

public record StorageAccountResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    string Kind,
    string AccessTier,
    bool AllowBlobPublicAccess,
    bool EnableHttpsTrafficOnly,
    string MinimumTlsVersion,
    IReadOnlyCollection<CorsRuleResult> CorsRules,
    IReadOnlyCollection<CorsRuleResult> TableCorsRules,
    IReadOnlyCollection<BlobContainerResult> BlobContainers,
    IReadOnlyCollection<StorageQueueResult> Queues,
    IReadOnlyCollection<StorageTableResult> Tables,
    IReadOnlyCollection<StorageAccountEnvironmentConfigData> EnvironmentSettings,
    IReadOnlyCollection<BlobLifecycleRuleResult> LifecycleRules
);

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
    IReadOnlyList<string> AllowedOrigins,
    IReadOnlyList<string> AllowedMethods,
    IReadOnlyList<string> AllowedHeaders,
    IReadOnlyList<string> ExposedHeaders,
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
    IReadOnlyList<string> ContainerNames,
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
    IReadOnlyList<CorsRuleResult> CorsRules,
    IReadOnlyList<CorsRuleResult> TableCorsRules,
    IReadOnlyList<BlobContainerResult> BlobContainers,
    IReadOnlyList<StorageQueueResult> Queues,
    IReadOnlyList<StorageTableResult> Tables,
    IReadOnlyList<StorageAccountEnvironmentConfigData> EnvironmentSettings,
    IReadOnlyList<BlobLifecycleRuleResult> LifecycleRules,

    bool IsExisting = false

);

using InfraFlowSculptor.Contracts.StorageAccounts.Requests;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Responses;

/// <summary>Represents a Blob Container inside a Storage Account.</summary>
public record BlobContainerResponse(
    string Id,
    string Name,
    string PublicAccess
);

public record CorsRuleResponse(
    IReadOnlyCollection<string> AllowedOrigins,
    IReadOnlyCollection<string> AllowedMethods,
    IReadOnlyCollection<string> AllowedHeaders,
    IReadOnlyCollection<string> ExposedHeaders,
    int MaxAgeInSeconds
);

/// <summary>Represents a Storage Queue inside a Storage Account.</summary>
public record StorageQueueResponse(
    string Id,
    string Name
);

/// <summary>Represents a Storage Table inside a Storage Account.</summary>
public record StorageTableResponse(
    string Id,
    string Name
);

/// <summary>Represents a blob lifecycle management rule.</summary>
public record BlobLifecycleRuleResponse(
    string RuleName,
    IReadOnlyCollection<string> ContainerNames,
    int TimeToLiveInDays
);

/// <summary>Represents an Azure Storage Account resource with its sub-resources.</summary>
public record StorageAccountResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    string Kind,
    string AccessTier,
    bool AllowBlobPublicAccess,
    bool EnableHttpsTrafficOnly,
    string MinimumTlsVersion,
    IReadOnlyCollection<CorsRuleResponse> CorsRules,
    IReadOnlyCollection<CorsRuleResponse> TableCorsRules,
    IReadOnlyCollection<BlobContainerResponse> BlobContainers,
    IReadOnlyCollection<StorageQueueResponse> Queues,
    IReadOnlyCollection<StorageTableResponse> Tables,
    IReadOnlyCollection<StorageAccountEnvironmentConfigResponse> EnvironmentSettings,
    IReadOnlyCollection<BlobLifecycleRuleResponse> LifecycleRules
);

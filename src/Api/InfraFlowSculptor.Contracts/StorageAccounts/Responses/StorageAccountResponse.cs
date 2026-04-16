using InfraFlowSculptor.Contracts.StorageAccounts.Requests;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Responses;

/// <summary>Represents a Blob Container inside a Storage Account.</summary>
public record BlobContainerResponse(
    string Id,
    string Name,
    string PublicAccess
);

public record CorsRuleResponse(
    IReadOnlyList<string> AllowedOrigins,
    IReadOnlyList<string> AllowedMethods,
    IReadOnlyList<string> AllowedHeaders,
    IReadOnlyList<string> ExposedHeaders,
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
    IReadOnlyList<string> ContainerNames,
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
    IReadOnlyList<CorsRuleResponse> CorsRules,
    IReadOnlyList<CorsRuleResponse> TableCorsRules,
    IReadOnlyList<BlobContainerResponse> BlobContainers,
    IReadOnlyList<StorageQueueResponse> Queues,
    IReadOnlyList<StorageTableResponse> Tables,
    IReadOnlyList<StorageAccountEnvironmentConfigResponse> EnvironmentSettings,
    IReadOnlyList<BlobLifecycleRuleResponse> LifecycleRules
);

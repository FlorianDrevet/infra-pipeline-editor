using InfraFlowSculptor.Contracts.StorageAccounts.Requests;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Responses;

/// <summary>Represents a Blob Container inside a Storage Account.</summary>
public record BlobContainerResponse(
    Guid Id,
    string Name,
    string PublicAccess
);

/// <summary>Represents a Storage Queue inside a Storage Account.</summary>
public record StorageQueueResponse(
    Guid Id,
    string Name
);

/// <summary>Represents a Storage Table inside a Storage Account.</summary>
public record StorageTableResponse(
    Guid Id,
    string Name
);

/// <summary>Represents an Azure Storage Account resource with its sub-resources.</summary>
public record StorageAccountResponse(
    Guid Id,
    Guid ResourceGroupId,
    string Name,
    string Location,
    string Kind,
    string AccessTier,
    bool AllowBlobPublicAccess,
    bool EnableHttpsTrafficOnly,
    string MinimumTlsVersion,
    IReadOnlyList<BlobContainerResponse> BlobContainers,
    IReadOnlyList<StorageQueueResponse> Queues,
    IReadOnlyList<StorageTableResponse> Tables,
    IReadOnlyList<StorageAccountEnvironmentConfigResponse> EnvironmentSettings
);

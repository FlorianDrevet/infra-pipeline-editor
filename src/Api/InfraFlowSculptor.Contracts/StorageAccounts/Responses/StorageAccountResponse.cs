namespace InfraFlowSculptor.Contracts.StorageAccounts.Responses;

public record BlobContainerResponse(
    Guid Id,
    string Name,
    string PublicAccess
);

public record StorageQueueResponse(
    Guid Id,
    string Name
);

public record StorageTableResponse(
    Guid Id,
    string Name
);

public record StorageAccountResponse(
    Guid Id,
    Guid ResourceGroupId,
    string Name,
    string Location,
    string Sku,
    string Kind,
    string AccessTier,
    bool AllowBlobPublicAccess,
    bool EnableHttpsTrafficOnly,
    string MinimumTlsVersion,
    IReadOnlyList<BlobContainerResponse> BlobContainers,
    IReadOnlyList<StorageQueueResponse> Queues,
    IReadOnlyList<StorageTableResponse> Tables
);

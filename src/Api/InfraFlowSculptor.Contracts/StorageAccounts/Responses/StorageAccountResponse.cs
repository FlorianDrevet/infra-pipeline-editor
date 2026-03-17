namespace InfraFlowSculptor.Contracts.StorageAccounts.Responses;

/// <summary>Represents a Blob Container inside a Storage Account.</summary>
/// <param name="Id">Unique identifier of the Blob Container.</param>
/// <param name="Name">Display name of the Blob Container.</param>
/// <param name="PublicAccess">Public access level (e.g. "None", "Blob", "Container").</param>
public record BlobContainerResponse(
    Guid Id,
    string Name,
    string PublicAccess
);

/// <summary>Represents a Storage Queue inside a Storage Account.</summary>
/// <param name="Id">Unique identifier of the Storage Queue.</param>
/// <param name="Name">Display name of the Storage Queue.</param>
public record StorageQueueResponse(
    Guid Id,
    string Name
);

/// <summary>Represents a Storage Table inside a Storage Account.</summary>
/// <param name="Id">Unique identifier of the Storage Table.</param>
/// <param name="Name">Display name of the Storage Table.</param>
public record StorageTableResponse(
    Guid Id,
    string Name
);

/// <summary>Represents an Azure Storage Account resource with its sub-resources.</summary>
/// <param name="Id">Unique identifier of the Storage Account.</param>
/// <param name="ResourceGroupId">Identifier of the parent Resource Group.</param>
/// <param name="Name">Display name of the Storage Account.</param>
/// <param name="Location">Azure region where the Storage Account is deployed.</param>
/// <param name="Sku">Performance and replication tier (e.g. "Standard_LRS").</param>
/// <param name="Kind">Account kind (e.g. "StorageV2").</param>
/// <param name="AccessTier">Default access tier for blob data (e.g. "Hot", "Cool").</param>
/// <param name="AllowBlobPublicAccess">Whether anonymous public read access to blobs is allowed.</param>
/// <param name="EnableHttpsTrafficOnly">Whether all HTTP traffic is redirected to HTTPS.</param>
/// <param name="MinimumTlsVersion">Minimum accepted TLS version (e.g. "TLS1_2").</param>
/// <param name="BlobContainers">List of Blob Containers defined in this Storage Account.</param>
/// <param name="Queues">List of Storage Queues defined in this Storage Account.</param>
/// <param name="Tables">List of Storage Tables defined in this Storage Account.</param>
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

using InfraFlowSculptor.Contracts.StorageAccounts.Responses;

namespace InfraFlowSculptor.Contracts.ResourceGroups.Responses;

/// <summary>
/// Represents lightweight Storage Account child resources returned in a Resource Group list.
/// </summary>
/// <param name="BlobContainers">Blob containers defined under the Storage Account.</param>
/// <param name="Queues">Queues defined under the Storage Account.</param>
/// <param name="Tables">Tables defined under the Storage Account.</param>
public sealed record StorageAccountSubResourcesResponse(
    IReadOnlyList<BlobContainerResponse> BlobContainers,
    IReadOnlyList<StorageQueueResponse> Queues,
    IReadOnlyList<StorageTableResponse> Tables);
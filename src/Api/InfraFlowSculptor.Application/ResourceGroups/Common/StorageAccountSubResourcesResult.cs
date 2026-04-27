using InfraFlowSculptor.Application.StorageAccounts.Common;

namespace InfraFlowSculptor.Application.ResourceGroups.Common;

/// <summary>
/// Represents lightweight Storage Account child resources for list views.
/// </summary>
/// <param name="BlobContainers">Blob containers defined under the Storage Account.</param>
/// <param name="Queues">Queues defined under the Storage Account.</param>
/// <param name="Tables">Tables defined under the Storage Account.</param>
public sealed record StorageAccountSubResourcesResult(
    IReadOnlyList<BlobContainerResult> BlobContainers,
    IReadOnlyList<StorageQueueResult> Queues,
    IReadOnlyList<StorageTableResult> Tables);
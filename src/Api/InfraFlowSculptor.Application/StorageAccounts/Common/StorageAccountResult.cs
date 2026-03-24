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

public record StorageQueueResult(
    StorageQueueId Id,
    string Name
);

public record StorageTableResult(
    StorageTableId Id,
    string Name
);

public record StorageAccountResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<BlobContainerResult> BlobContainers,
    IReadOnlyList<StorageQueueResult> Queues,
    IReadOnlyList<StorageTableResult> Tables,
    IReadOnlyList<StorageAccountEnvironmentConfigData> EnvironmentSettings
);

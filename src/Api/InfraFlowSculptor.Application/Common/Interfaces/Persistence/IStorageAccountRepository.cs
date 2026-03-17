using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Shared.Application.Interfaces;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IStorageAccountRepository : IRepository<StorageAccount>
{
    Task<StorageAccount?> GetByIdWithSubResourcesAsync(AzureResourceId id, CancellationToken cancellationToken = default);
    Task<List<StorageAccount>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);

    Task<BlobContainer> AddBlobContainerAsync(BlobContainer container);
    Task<bool> RemoveBlobContainerAsync(AzureResourceId storageAccountId, BlobContainerId id);

    Task<StorageQueue> AddQueueAsync(StorageQueue queue);
    Task<bool> RemoveQueueAsync(AzureResourceId storageAccountId, StorageQueueId id);

    Task<StorageTable> AddTableAsync(StorageTable table);
    Task<bool> RemoveTableAsync(AzureResourceId storageAccountId, StorageTableId id);
}

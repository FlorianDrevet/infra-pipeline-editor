using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;

public class StorageQueue : Entity<StorageQueueId>
{
    public AzureResourceId StorageAccountId { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    private StorageQueue(StorageQueueId id) : base(id)
    {
    }

    private StorageQueue()
    {
    }

    public static StorageQueue Create(AzureResourceId storageAccountId, string name)
    {
        return new StorageQueue(StorageQueueId.CreateUnique())
        {
            StorageAccountId = storageAccountId,
            Name = name
        };
    }
}

using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;

/// <summary>Represents a queue sub-resource within an Azure Storage Account.</summary>
public class StorageQueue : Entity<StorageQueueId>
{
    /// <summary>Gets the parent Storage Account identifier.</summary>
    public AzureResourceId StorageAccountId { get; private set; } = null!;

    /// <summary>Gets the queue name.</summary>
    public string Name { get; private set; } = null!;

    private StorageQueue(StorageQueueId id) : base(id)
    {
    }

    private StorageQueue()
    {
    }

    /// <summary>Creates a new <see cref="StorageQueue"/> with a generated identifier.</summary>
    public static StorageQueue Create(AzureResourceId storageAccountId, string name)
    {
        return new StorageQueue(StorageQueueId.CreateUnique())
        {
            StorageAccountId = storageAccountId,
            Name = name
        };
    }
}

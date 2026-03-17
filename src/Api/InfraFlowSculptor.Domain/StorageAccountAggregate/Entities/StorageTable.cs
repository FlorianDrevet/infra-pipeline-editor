using BicepGenerator.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;

public class StorageTable : Entity<StorageTableId>
{
    public AzureResourceId StorageAccountId { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    private StorageTable(StorageTableId id) : base(id)
    {
    }

    private StorageTable()
    {
    }

    public static StorageTable Create(AzureResourceId storageAccountId, string name)
    {
        return new StorageTable(StorageTableId.CreateUnique())
        {
            StorageAccountId = storageAccountId,
            Name = name
        };
    }
}

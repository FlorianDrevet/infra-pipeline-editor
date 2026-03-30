using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;

/// <summary>Represents a table sub-resource within an Azure Storage Account.</summary>
public class StorageTable : Entity<StorageTableId>
{
    /// <summary>Gets the parent Storage Account identifier.</summary>
    public AzureResourceId StorageAccountId { get; private set; } = null!;

    /// <summary>Gets the table name.</summary>
    public string Name { get; private set; } = null!;

    private StorageTable(StorageTableId id) : base(id)
    {
    }

    private StorageTable()
    {
    }

    /// <summary>Creates a new <see cref="StorageTable"/> with a generated identifier.</summary>
    public static StorageTable Create(AzureResourceId storageAccountId, string name)
    {
        return new StorageTable(StorageTableId.CreateUnique())
        {
            StorageAccountId = storageAccountId,
            Name = name
        };
    }
}

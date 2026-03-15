using BicepGenerator.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;

public class BlobContainer : Entity<BlobContainerId>
{
    public AzureResourceId StorageAccountId { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public BlobContainerPublicAccess PublicAccess { get; private set; } = null!;

    private BlobContainer(BlobContainerId id) : base(id)
    {
    }

    private BlobContainer()
    {
    }

    public static BlobContainer Create(
        AzureResourceId storageAccountId,
        string name,
        BlobContainerPublicAccess publicAccess)
    {
        return new BlobContainer(BlobContainerId.CreateUnique())
        {
            StorageAccountId = storageAccountId,
            Name = name,
            PublicAccess = publicAccess
        };
    }
}

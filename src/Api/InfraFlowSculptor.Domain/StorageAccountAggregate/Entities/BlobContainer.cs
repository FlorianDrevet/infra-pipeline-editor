using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;

/// <summary>Represents a blob container sub-resource within an Azure Storage Account.</summary>
public class BlobContainer : Entity<BlobContainerId>
{
    /// <summary>Gets the parent Storage Account identifier.</summary>
    public AzureResourceId StorageAccountId { get; private set; } = null!;

    /// <summary>Gets the container name.</summary>
    public string Name { get; private set; } = null!;

    /// <summary>Gets the public access level for this container.</summary>
    public BlobContainerPublicAccess PublicAccess { get; private set; } = null!;

    private BlobContainer(BlobContainerId id) : base(id)
    {
    }

    private BlobContainer()
    {
    }

    /// <summary>
    /// Updates the public access level for this blob container.
    /// </summary>
    /// <param name="publicAccess">The new public access level to apply.</param>
    public void UpdatePublicAccess(BlobContainerPublicAccess publicAccess)
    {
        PublicAccess = publicAccess;
    }

    /// <summary>Creates a new <see cref="BlobContainer"/> with a generated identifier.</summary>
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

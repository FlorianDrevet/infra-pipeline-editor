using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;

/// <summary>
/// Represents a <c>Microsoft.ManagedIdentity/userAssignedIdentities</c> Azure resource.
/// This resource has no per-environment settings.
/// </summary>
public sealed class UserAssignedIdentity : AzureResource
{
    private UserAssignedIdentity() { }

    /// <summary>
    /// Updates the mutable properties of this user-assigned identity.
    /// </summary>
    /// <param name="name">The new resource name.</param>
    /// <param name="location">The new Azure location.</param>
    public void Update(Name name, Location location)
    {
        Name = name;
        Location = location;
    }

    /// <summary>
    /// Creates a new <see cref="UserAssignedIdentity"/> with a unique identifier.
    /// </summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="name">The resource name.</param>
    /// <param name="location">The Azure location.</param>
    /// <param name="isExisting">When <c>true</c>, this resource already exists in Azure and is not deployed by this project.</param>
    /// <returns>A new <see cref="UserAssignedIdentity"/> instance.</returns>
    public static UserAssignedIdentity Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        bool isExisting = false)
    {
        return new UserAssignedIdentity
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            IsExisting = isExisting
        };
    }
}

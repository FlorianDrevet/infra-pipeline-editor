using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ResourceGroupAggregate;

/// <summary>
/// Represents an Azure Resource Group that contains Azure resources.
/// Enforces location consistency and resource limits (max 800 resources).
/// </summary>
public sealed class ResourceGroup : AggregateRoot<ResourceGroupId>
{
    /// <summary>Gets the display name of this resource group.</summary>
    public Name Name { get; private set; } = null!;

    /// <summary>Gets the parent infrastructure configuration identifier.</summary>
    public InfrastructureConfigId InfraConfigId { get; private set; } = null!;

    /// <summary>Navigation property to the parent infrastructure configuration.</summary>
    public InfrastructureConfig InfraConfig { get; private set; } = null!;

    /// <summary>Gets the Azure region for this resource group.</summary>
    public Location Location { get; private set; } = null!;

    private readonly List<AzureResource> _resources = [];

    /// <summary>Gets the Azure resources contained in this resource group.</summary>
    public IReadOnlyCollection<AzureResource> Resources => _resources.AsReadOnly();

    private ResourceGroup(ResourceGroupId id, Name name, InfrastructureConfigId infraConfigId, Location location)
        : base(id)
    {
        Name = name;
        InfraConfigId = infraConfigId;
        Location = location;
    }
    
    /// <summary>Creates a new <see cref="ResourceGroup"/> with a generated identifier.</summary>
    public static ResourceGroup Create(Name name, InfrastructureConfigId infraConfigId, Location location)
    {
        return new ResourceGroup(ResourceGroupId.CreateUnique(), name, infraConfigId, location);
    }
    
    /// <summary>
    /// Adds a resource to this resource group. Validates location consistency,
    /// name uniqueness, and the 800-resource limit.
    /// </summary>
    public ErrorOr<Success> AddResource(AzureResource resource)
    {
        if (resource.Location != Location)
            return Errors.ResourceGroup.AddResource.ResourceNotInSameLocation();

        if (_resources.Any(r => r.Name == resource.Name))
            return Errors.ResourceGroup.AddResource.ResourceAlreadyInGroup();
        
        if (_resources.Count >= 800)
            return Errors.ResourceGroup.AddResource.ResourceGroupResourceLimitReached();

        _resources.Add(resource);
        return Result.Success;
    }
    
    /// <summary>
    /// Removes a resource from this resource group. Returns an error if the resource
    /// is not in the group or is required as a dependency by other resources.
    /// </summary>
    public ErrorOr<Success> RemoveResource(AzureResource resource)
    {
        if (!_resources.Contains(resource))
            return Errors.ResourceGroup.RemoveResource.ResourceNotInGroup();

        var isDependency = _resources.Any(r => r.DependsOn.Contains(resource));
        if (isDependency)
            return Errors.ResourceGroup.RemoveResource.ResourceIsDependency();

        _resources.Remove(resource);
        return Result.Success;
    }

    /// <summary>EF Core constructor.</summary>
    public ResourceGroup()
    {
    }
}
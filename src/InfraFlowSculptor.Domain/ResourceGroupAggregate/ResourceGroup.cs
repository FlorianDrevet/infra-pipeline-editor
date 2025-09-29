using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ResourceGroupAggregate;

public sealed class ResourceGroup : AggregateRoot<ResourceGroupId>
{
    public Name Name { get; private set; } = null!;
    
    public InfrastructureConfigId InfraConfigId { get; set; } = null!;
    public InfrastructureConfig InfraConfig { get; set; } = null!;

    public Location Location { get; set; } = null!;

    private List<AzureResource> _resources = new();
    public IReadOnlyCollection<AzureResource> Resources => _resources.AsReadOnly();

    private ResourceGroup(ResourceGroupId id, Name name, InfrastructureConfigId infraConfigId, Location location)
        : base(id)
    {
        Name = name;
        InfraConfigId = infraConfigId;
        Location = location;
    }
    
    public static ResourceGroup Create(Name name, InfrastructureConfigId infraConfigId, Location location)
    {
        return new ResourceGroup(ResourceGroupId.CreateUnique(), name, infraConfigId, location);
    }
    
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
    
    public ErrorOr<Success> RemoveResource(AzureResource resource)
    {
        if (!_resources.Contains(resource))
            return Errors.ResourceGroup.RemoveResource.ResourceNotInGroup();

        var isDependency = _resources.Any(r => r.Dependencies.Contains(resource));
        if (isDependency)
            return Errors.ResourceGroup.RemoveResource.ResourceIsDependency();

        _resources.Remove(resource);
        return Result.Success;
    }

    public ResourceGroup()
    {
    }
}
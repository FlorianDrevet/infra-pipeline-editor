using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ResourceGroupAggregate;

public sealed class ResourceGroup : AggregateRoot<ResourceGroupId>
{
    public string Name { get; private set; } = null!;
    public Location Location { get; private set; } = null!;
    
    public string? Prefix { get; private set; } = null!;
    public string? Suffix { get; private set; } = null!;
    
    private List<AzureResource> _azureResources = new List<AzureResource>();
    public IReadOnlyList<AzureResource> AzureResources => _azureResources.AsReadOnly();

    private ResourceGroup(ResourceGroupId id)
        : base(id)
    {

    }

    public static ResourceGroup Create()
    {
        return new ResourceGroup(ResourceGroupId.CreateUnique());
    }

    public ResourceGroup()
    {
    }
}
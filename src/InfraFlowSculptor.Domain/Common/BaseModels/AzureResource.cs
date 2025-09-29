using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.BaseModels;

public class AzureResource : AggregateRoot<AzureResourceId>
{
    public ResourceGroupId ResourceGroupId { get; set; }
    public ResourceGroup ResourceGroup { get; set; } = null!;

    public Name Name { get; set; } = null!;
    public Location Location { get; set; } = null!;
    
    private List<AzureResource> _dependencies = new List<AzureResource>();
    public IReadOnlyList<AzureResource> Dependencies => _dependencies.AsReadOnly();

    //public ManagedIdentity? ManagedIdentity { get; set; }

    private AzureResource(AzureResourceId id)
        : base(id)
    {

    }
    
    public static AzureResource Create(ResourceGroupId resourceGroupId, Name name, Location location, List<AzureResource> dependencies)
    {
        return new AzureResource(AzureResourceId.CreateUnique())
        {
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            _dependencies = dependencies
        };
    }
    

    public AzureResource()
    {
    }
}
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.BaseModels;

public class AzureResource : AggregateRoot<AzureResourceId>
{
    public required ResourceGroupId ResourceGroupId { get; set; }
    public ResourceGroup ResourceGroup { get; set; } = null!;

    public required Name Name { get; set; }
    public required Location Location { get; set; }
    
    private List<AzureResource> _dependencies = new List<AzureResource>();
    public IReadOnlyList<AzureResource> Dependencies => _dependencies.AsReadOnly();

    //public ManagedIdentity? ManagedIdentity { get; set; }

    protected AzureResource(AzureResourceId id)
        : base(id)
    {

    }

    public AzureResource()
    {
    }
}
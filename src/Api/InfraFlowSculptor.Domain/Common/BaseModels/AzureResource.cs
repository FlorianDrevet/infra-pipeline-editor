using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels;

public class AzureResource : AggregateRoot<AzureResourceId>
{
    public required ResourceGroupId ResourceGroupId { get; set; }
    public ResourceGroup ResourceGroup { get; set; } = null!;

    public required Name Name { get; set; }
    public required Location Location { get; set; }
    
    private List<AzureResource> _dependsOn = new List<AzureResource>();
    public IReadOnlyList<AzureResource> DependsOn => _dependsOn.AsReadOnly();

    //public ManagedIdentity? ManagedIdentity { get; set; }
    
    // Méthode métier
    public void AddDependency(AzureResource resource)
    {
        if (resource.Id == Id)
            throw new InvalidOperationException("Une ressource ne peut pas dépendre d'elle-même.");

        if (_dependsOn.Any(r => r.Id == resource.Id))
            return;

        _dependsOn.Add(resource);
    }

    protected AzureResource()
    {
    }
}
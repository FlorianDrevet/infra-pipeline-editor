using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate;

public sealed class InfrastructureConfig : AggregateRoot<InfrastructureConfigId>
{
    public Name Name { get; private set; } = null!;
    private List<ResourceGroup> _resourceGroups { get; set; } = new();
    public IReadOnlyList<ResourceGroup> ResourceGroups => _resourceGroups.AsReadOnly();
    
    //public List<EnvironmentVariable> Variables { get; set; } = new();

    private InfrastructureConfig(InfrastructureConfigId id, Name name)
        : base(id)
    {
        Name = name;
    }

    public static InfrastructureConfig Create(Name name)
    {
        return new InfrastructureConfig(InfrastructureConfigId.CreateUnique(), name);
    }

    public InfrastructureConfig()
    {
    }
    
    public bool AddResourceGroup(ResourceGroup resourceGroup)
    {
        if (_resourceGroups.Any(rg => rg.Name == resourceGroup.Name))
            return false;
        
        _resourceGroups.Add(resourceGroup);
        return true;
    }
    
    public bool RemoveResourceGroup(ResourceGroup resourceGroup)
    {
        return _resourceGroups.Remove(resourceGroup);
    }
}
using System.Security.AccessControl;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.BaseModels;

public abstract class AzureResourceConfiguration<T>: AggregateRoot<T> where T : notnull
{
    public Location Location { get; protected set; } = null!;
    public string Name { get; protected set; } = null!;

    private List<(string, ResourceType)> _dependsOn = new();
    public IReadOnlyList<(string, ResourceType)> DependsOn => _dependsOn.AsReadOnly();
    
    protected AzureResourceConfiguration(T id, string name, Location location)
        : base(id)
    {
        Location = location;
        Name = name;
    }

    public AzureResourceConfiguration()
    {
        
    }
}
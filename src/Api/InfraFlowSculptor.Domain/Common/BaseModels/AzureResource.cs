using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
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
    
    protected virtual IReadOnlyCollection<ParameterUsage> AllowedParameterUsages { get; }

    private readonly List<ResourceParameterUsage> _parameterUsages = new();
    public IReadOnlyCollection<ResourceParameterUsage> ParameterUsages => _parameterUsages;
    
    private readonly List<InputOutputLink> _inputs = new();
    public IReadOnlyCollection<InputOutputLink> Inputs => _inputs;

    private readonly List<InputOutputLink> _outputs = new();
    public IReadOnlyCollection<InputOutputLink> Outputs => _outputs;


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
    
    protected void AddParameterUsage(
        ParameterDefinition parameter,
        ParameterUsage usage)
    {
        //TODO
        if (!AllowedParameterUsages.Contains(usage))
            throw new Exception(
                $"{GetType().Name} does not support parameter usage '{usage}'");

        _parameterUsages.Add(
            new ResourceParameterUsage(Id, parameter.Id, usage));
    }

    protected AzureResource()
    {
    }
}
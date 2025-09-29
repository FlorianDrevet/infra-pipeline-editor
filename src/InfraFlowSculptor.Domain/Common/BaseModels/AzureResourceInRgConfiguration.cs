using InfraFlowSculptor.BicepDirector.Enums;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;

namespace InfraFlowSculptor.Domain.Common.BaseModels;

public abstract class AzureResourceInRgConfiguration<T>: AzureResourceConfiguration<T> where T : notnull
{
    public DeploymentKind DeploymentKind { get; protected set; }
    public ResourceGroup ResourceGroup { get; protected set; } = null!;
    public bool IsExisting { get; protected set; }
    
    protected AzureResourceInRgConfiguration(
        T id, 
        string name,
        Location location,
        DeploymentKind deploymentKind,
        ResourceGroup resourceGroup,
        bool isExisting)
        : base(id, name, location)
    {
        DeploymentKind = deploymentKind;
        ResourceGroup = resourceGroup;
        IsExisting = isExisting;
    }

    public AzureResourceInRgConfiguration()
    {
        
    }
}
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.KeyVaultAggregate;

public class KeyVault: AzureResource
{
    public required Sku Sku { get; set; }
    
    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        new[]
        {
            ParameterUsage.Secret
        };

    public void AddSecret(ParameterDefinition parameter)
    {
        //TODO
        if (!parameter.IsSecret)
            throw new Exception("Only secret parameters can be stored in KeyVault");

        AddParameterUsage(parameter, ParameterUsage.Secret);
    }
    
    private KeyVault()
    {
    }
    
    public static KeyVault Create(ResourceGroupId resourceGroupId, Name name, Location location, Sku sku)
    {
        return new KeyVault
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            Sku = sku
        };
    }
}
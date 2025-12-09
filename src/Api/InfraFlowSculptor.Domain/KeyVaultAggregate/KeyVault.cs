using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.KeyVaultAggregate;

public class KeyVault: AzureResource
{
    public required Sku Sku { get; set; }
    
    public KeyVault(): base()
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
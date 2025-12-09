using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.KeyVaultAggregate;

public class KeyVault: AzureResource
{
    public string Sku { get; set; } = string.Empty;
    
    public KeyVault(): base()
    {
    }
}
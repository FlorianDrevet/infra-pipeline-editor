namespace InfraFlowSculptor.Domain.KeyVaultAggregate.Constants;

/// <summary>Available Bicep output types for a Key Vault resource.</summary>
public enum KeyVaultOutputs
{
    KeyVaultName,
    KeyVaultResourceId,
    KeyVaultUri,
    KeyVaultSku,
    KeyVaultTenantId,
    KeyVaultAccessPolicies
}
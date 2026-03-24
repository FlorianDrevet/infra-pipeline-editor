using InfraFlowSculptor.Contracts.KeyVaults.Requests;

namespace InfraFlowSculptor.Contracts.KeyVaults.Responses;

/// <summary>Represents an Azure Key Vault resource.</summary>
/// <param name="Id">Unique identifier of the Key Vault.</param>
/// <param name="ResourceGroupId">Identifier of the parent Resource Group.</param>
/// <param name="Name">Display name of the Key Vault.</param>
/// <param name="Location">Azure region where the Key Vault is deployed.</param>
/// <param name="Sku">Pricing tier of the Key Vault (e.g. "Standard", "Premium").</param>
/// <param name="EnvironmentSettings">Per-environment typed configuration overrides.</param>
public record KeyVaultResponse(
    Guid Id,
    Guid ResourceGroupId,
    string Name,
    string Location,
    IReadOnlyList<KeyVaultEnvironmentConfigResponse> EnvironmentSettings
);
using InfraFlowSculptor.Contracts.KeyVaults.Requests;

namespace InfraFlowSculptor.Contracts.KeyVaults.Responses;

/// <summary>Represents an Azure Key Vault resource.</summary>
/// <param name="Id">Unique identifier of the Key Vault.</param>
/// <param name="ResourceGroupId">Identifier of the parent Resource Group.</param>
/// <param name="Name">Display name of the Key Vault.</param>
/// <param name="Location">Azure region where the Key Vault is deployed.</param>
/// <param name="EnableRbacAuthorization">Whether RBAC authorization is enabled for data-plane access.</param>
/// <param name="EnabledForDeployment">Whether the vault is enabled for deployment.</param>
/// <param name="EnabledForDiskEncryption">Whether the vault is enabled for disk encryption.</param>
/// <param name="EnabledForTemplateDeployment">Whether the vault is enabled for ARM template deployment.</param>
/// <param name="EnablePurgeProtection">Whether purge protection is enabled.</param>
/// <param name="EnableSoftDelete">Whether soft delete is enabled.</param>
/// <param name="EnvironmentSettings">Per-environment typed configuration overrides.</param>
public record KeyVaultResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    bool EnableRbacAuthorization,
    bool EnabledForDeployment,
    bool EnabledForDiskEncryption,
    bool EnabledForTemplateDeployment,
    bool EnablePurgeProtection,
    bool EnableSoftDelete,
    IReadOnlyCollection<KeyVaultEnvironmentConfigResponse> EnvironmentSettings
);
using InfraFlowSculptor.BicepDirector.Enums;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;

namespace InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate;

public sealed class KeyVault : AzureResourceWithIdentityConfiguration<KeyVaultId>
{
    public KeyVaultSku Sku { get; private set; } = null!;

    // Default configuration
    public bool EnableRbacAuthorization { get; private set; } = false;
    public SoftDelete SoftDelete { get; private set; }
    public bool EnablePurgeProtection { get; private set; } = true;
    
    private List<SecretUserEntity> _userSecrets = new();
    public IReadOnlyList<SecretUserEntity> UserSecrets => _userSecrets.AsReadOnly();
    
    private List<SecretAzureResourceEntity> _azureResourceSecrets = new();
    public IReadOnlyList<SecretAzureResourceEntity> AzureResourceSecrets => _azureResourceSecrets.AsReadOnly();
    
    private KeyVault(
        KeyVaultId id,
        string name,
        Location location,
        DeploymentKind deploymentKind,
        ResourceGroup resourceGroup,
        bool isExisting,
        bool isSystemAssigned,
        Guid userAssignedIdentity,
        List<RightAssignation> rightAssignations,
        KeyVaultSku sku,
        List<SecretUserEntity> secretUserEntities,
        List<SecretAzureResourceEntity> azureResourceEntities,
        bool enableRbacAuthorization,
        bool enablePurgeProtection,
        SoftDelete softDelete) : base(id, name, location, deploymentKind, resourceGroup, isExisting, isSystemAssigned, userAssignedIdentity, rightAssignations)
    {
        Sku = sku;
        _userSecrets = secretUserEntities;
        _azureResourceSecrets = azureResourceEntities;
        EnablePurgeProtection = enablePurgeProtection;
        EnableRbacAuthorization = enableRbacAuthorization;
        SoftDelete = softDelete;
    }

    public static KeyVault Create(string name,
        Location location,
        DeploymentKind deploymentKind,
        ResourceGroup resourceGroup,
        bool isExisting,
        bool isSystemAssigned,
        Guid userAssignedIdentity,
        List<RightAssignation> rightAssignations,
        KeyVaultSku sku,
        List<SecretUserEntity> secretUserEntities,
        List<SecretAzureResourceEntity> azureResourceEntities,
        bool enableRbacAuthorization,
        bool enablePurgeProtection,
        SoftDelete softDelete)
    {
        return new KeyVault(KeyVaultId.CreateUnique(), name, location, deploymentKind, resourceGroup, isExisting,
            isSystemAssigned, userAssignedIdentity, rightAssignations, sku, secretUserEntities, azureResourceEntities,
            enableRbacAuthorization, enablePurgeProtection, softDelete);
    }

    public KeyVault()
    {
    }
}
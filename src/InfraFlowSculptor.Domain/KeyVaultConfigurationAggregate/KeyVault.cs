using InfraFlowSculptor.BicepDirector.Enums;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;

namespace InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate;

public sealed class KeyVault : AzureResourceWithIdentityConfiguration<KeyVaultId>
{
    private KeyVault(
        KeyVaultId id,
        string name,
        Location location,
        DeploymentKind deploymentKind,
        ResourceGroup resourceGroup,
        bool isExisting,
        bool isSystemAssigned,
        Guid userAssignedIdentity,
        List<RightAssignation> rightAssignations) : base(id, name, location, deploymentKind, resourceGroup, isExisting, isSystemAssigned, userAssignedIdentity, rightAssignations)
    {

    }

    public static KeyVault Create()
    {
        return new KeyVault(KeyVaultId.CreateUnique());
    }

    public KeyVault()
    {
    }
}
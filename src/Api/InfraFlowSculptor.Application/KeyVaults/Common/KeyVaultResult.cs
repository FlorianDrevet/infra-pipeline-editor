using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.KeyVaults.Common;

public record KeyVaultResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    bool EnableRbacAuthorization,
    bool EnabledForDeployment,
    bool EnabledForDiskEncryption,
    bool EnabledForTemplateDeployment,
    bool EnablePurgeProtection,
    bool EnableSoftDelete,
    IReadOnlyList<KeyVaultEnvironmentConfigData> EnvironmentSettings
);
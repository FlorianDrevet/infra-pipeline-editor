using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.UpdateKeyVault;

public record UpdateKeyVaultCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    bool EnableRbacAuthorization = true,
    bool EnabledForDeployment = false,
    bool EnabledForDiskEncryption = false,
    bool EnabledForTemplateDeployment = false,
    bool EnablePurgeProtection = true,
    bool EnableSoftDelete = true,
    IReadOnlyCollection<KeyVaultEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<KeyVaultResult>;

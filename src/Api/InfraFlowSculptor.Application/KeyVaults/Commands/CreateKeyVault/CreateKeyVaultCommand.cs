using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;

public record CreateKeyVaultCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    bool EnableRbacAuthorization = true,
    bool EnabledForDeployment = false,
    bool EnabledForDiskEncryption = false,
    bool EnabledForTemplateDeployment = false,
    bool EnablePurgeProtection = true,
    bool EnableSoftDelete = true,
    IReadOnlyList<KeyVaultEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<KeyVaultResult>>;
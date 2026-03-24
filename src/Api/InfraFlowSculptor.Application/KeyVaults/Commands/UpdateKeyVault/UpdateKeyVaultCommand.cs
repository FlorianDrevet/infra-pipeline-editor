using ErrorOr;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.UpdateKeyVault;

public record UpdateKeyVaultCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    IReadOnlyList<KeyVaultEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<KeyVaultResult>>;

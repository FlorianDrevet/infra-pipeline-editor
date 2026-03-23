using InfraFlowSculptor.Application.Common;
using ErrorOr;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.UpdateKeyVault;

public record UpdateKeyVaultCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    Sku Sku,
    IReadOnlyList<EnvironmentConfigData>? EnvironmentConfigs = null
) : IRequest<ErrorOr<KeyVaultResult>>;

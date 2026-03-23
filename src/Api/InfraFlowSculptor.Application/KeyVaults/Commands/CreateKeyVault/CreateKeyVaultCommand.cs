using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;

public record CreateKeyVaultCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    Sku Sku,
    IReadOnlyList<KeyVaultEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<KeyVaultResult>>;
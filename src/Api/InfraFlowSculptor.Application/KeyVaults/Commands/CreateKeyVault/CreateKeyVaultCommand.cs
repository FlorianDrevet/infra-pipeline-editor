using InfraFlowSculptor.Domain.Common.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;

public record CreateKeyVaultCommand(
    Guid ResourceGroupId,
    string Name,
    Location Location,
    string Sku
) : IRequest<CreateKeyVaultResult>;
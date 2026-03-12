using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.DeleteKeyVault;

public record DeleteKeyVaultCommand(
    AzureResourceId Id
) : IRequest<ErrorOr<Deleted>>;

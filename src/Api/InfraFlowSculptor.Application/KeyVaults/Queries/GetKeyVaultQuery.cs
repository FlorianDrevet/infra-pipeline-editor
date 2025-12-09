using ErrorOr;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Queries;

public record GetKeyVaultQuery(
    AzureResourceId Id
) : IRequest<ErrorOr<KeyVaultResult>>;
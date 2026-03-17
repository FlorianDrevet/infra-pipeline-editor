using ErrorOr;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Queries;

public record ListKeyVaultsQuery(
    ResourceGroupId ResourceGroupId
) : IRequest<ErrorOr<List<KeyVaultResult>>>;

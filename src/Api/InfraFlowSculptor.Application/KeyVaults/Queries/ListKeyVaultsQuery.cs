using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.KeyVaults.Queries;

public record ListKeyVaultsQuery(
    ResourceGroupId ResourceGroupId
) : IQuery<List<KeyVaultResult>>;

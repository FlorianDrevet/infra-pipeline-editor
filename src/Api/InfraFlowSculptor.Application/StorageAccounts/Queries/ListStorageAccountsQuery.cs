using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.StorageAccounts.Queries;

public record ListStorageAccountsQuery(
    ResourceGroupId ResourceGroupId
) : IQuery<List<StorageAccountResult>>;

using ErrorOr;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Queries;

public record ListStorageAccountsQuery(
    ResourceGroupId ResourceGroupId
) : IRequest<ErrorOr<List<StorageAccountResult>>>;

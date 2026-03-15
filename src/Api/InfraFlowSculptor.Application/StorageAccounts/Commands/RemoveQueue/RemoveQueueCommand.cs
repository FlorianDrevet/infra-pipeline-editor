using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveQueue;

public record RemoveQueueCommand(
    AzureResourceId StorageAccountId,
    StorageQueueId QueueId
) : IRequest<ErrorOr<Deleted>>;

using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveQueue;

public record RemoveQueueCommand(
    AzureResourceId StorageAccountId,
    StorageQueueId QueueId
) : ICommand<Deleted>;

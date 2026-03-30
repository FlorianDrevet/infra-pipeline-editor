using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveTable;

public record RemoveTableCommand(
    AzureResourceId StorageAccountId,
    StorageTableId TableId
) : ICommand<Deleted>;

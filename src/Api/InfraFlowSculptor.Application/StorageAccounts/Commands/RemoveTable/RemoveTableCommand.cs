using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveTable;

public record RemoveTableCommand(
    AzureResourceId StorageAccountId,
    StorageTableId TableId
) : IRequest<ErrorOr<Deleted>>;

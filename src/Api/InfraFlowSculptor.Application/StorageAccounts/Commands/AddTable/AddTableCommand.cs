using ErrorOr;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.AddTable;

public record AddTableCommand(
    AzureResourceId StorageAccountId,
    string Name
) : IRequest<ErrorOr<StorageAccountResult>>;

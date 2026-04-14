using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.AddTable;

public record AddTableCommand(
    AzureResourceId StorageAccountId,
    string Name
) : ICommand<StorageAccountResult>;

using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.DeleteStorageAccount;

public record DeleteStorageAccountCommand(
    AzureResourceId Id
) : ICommand<Deleted>;

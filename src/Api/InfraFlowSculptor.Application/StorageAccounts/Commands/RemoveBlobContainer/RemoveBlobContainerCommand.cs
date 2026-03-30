using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveBlobContainer;

public record RemoveBlobContainerCommand(
    AzureResourceId StorageAccountId,
    BlobContainerId ContainerId
) : ICommand<Deleted>;

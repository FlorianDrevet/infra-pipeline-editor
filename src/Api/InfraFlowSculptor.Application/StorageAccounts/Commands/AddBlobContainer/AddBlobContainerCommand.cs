using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.AddBlobContainer;

public record AddBlobContainerCommand(
    AzureResourceId StorageAccountId,
    string Name,
    BlobContainerPublicAccess PublicAccess
) : ICommand<StorageAccountResult>;

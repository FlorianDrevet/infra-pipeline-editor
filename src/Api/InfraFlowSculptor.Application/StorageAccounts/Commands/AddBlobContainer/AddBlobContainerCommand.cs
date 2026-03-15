using ErrorOr;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.AddBlobContainer;

public record AddBlobContainerCommand(
    AzureResourceId StorageAccountId,
    string Name,
    BlobContainerPublicAccess PublicAccess
) : IRequest<ErrorOr<StorageAccountResult>>;

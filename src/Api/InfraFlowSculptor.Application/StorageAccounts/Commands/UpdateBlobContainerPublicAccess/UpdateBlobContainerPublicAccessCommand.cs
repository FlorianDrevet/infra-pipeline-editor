using ErrorOr;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateBlobContainerPublicAccess;

/// <summary>
/// Updates the public access level of an existing blob container within a storage account.
/// </summary>
public record UpdateBlobContainerPublicAccessCommand(
    AzureResourceId StorageAccountId,
    BlobContainerId ContainerId,
    BlobContainerPublicAccess PublicAccess
) : IRequest<ErrorOr<StorageAccountResult>>;

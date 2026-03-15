using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.AddBlobContainer;

public class AddBlobContainerCommandHandler(
    IStorageAccountRepository storageAccountRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<AddBlobContainerCommand, ErrorOr<StorageAccountResult>>
{
    public Task<ErrorOr<StorageAccountResult>> Handle(AddBlobContainerCommand request, CancellationToken cancellationToken) =>
        StorageAccountAccessHelper.AddSubResourceAndReloadAsync(
            request.StorageAccountId, storageAccountRepository, resourceGroupRepository, infraConfigRepository, currentUser,
            sa => sa.AddBlobContainer(request.Name, request.PublicAccess),
            storageAccountRepository.AddBlobContainerAsync,
            mapper, cancellationToken);
}

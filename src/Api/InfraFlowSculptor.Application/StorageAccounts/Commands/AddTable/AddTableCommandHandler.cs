using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.AddTable;

public class AddTableCommandHandler(
    IStorageAccountRepository storageAccountRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<AddTableCommand, ErrorOr<StorageAccountResult>>
{
    public async Task<ErrorOr<StorageAccountResult>> Handle(AddTableCommand request, CancellationToken cancellationToken)
    {
        var saResult = await StorageAccountAccessHelper.GetWithWriteAccessAsync(
            request.StorageAccountId, storageAccountRepository, resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);

        if (saResult.IsError)
            return saResult.Errors;

        var table = saResult.Value.AddTable(request.Name);

        await storageAccountRepository.AddTableAsync(table);

        var updated = await storageAccountRepository.GetByIdWithSubResourcesAsync(request.StorageAccountId, cancellationToken);
        if (updated is null)
            return Errors.StorageAccount.NotFoundError(request.StorageAccountId);

        return mapper.Map<StorageAccountResult>(updated);
    }
}

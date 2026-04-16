using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Queries;

public class ListStorageAccountsQueryHandler(
    IStorageAccountRepository storageAccountRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<ListStorageAccountsQuery, List<StorageAccountResult>>
{
    public async Task<ErrorOr<List<StorageAccountResult>>> Handle(ListStorageAccountsQuery query, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(query.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(query.ResourceGroupId);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var storageAccounts = await storageAccountRepository.GetByResourceGroupIdAsync(query.ResourceGroupId, cancellationToken);

        return storageAccounts.Select(sa => mapper.Map<StorageAccountResult>(sa)).ToList();
    }
}

using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Queries;

public class GetStorageAccountQueryHandler(
    IStorageAccountRepository storageAccountRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<GetStorageAccountQuery, ErrorOr<StorageAccountResult>>
{
    public async Task<ErrorOr<StorageAccountResult>> Handle(GetStorageAccountQuery query, CancellationToken cancellationToken)
    {
        var ctx = new StorageAccountAccessContext(query.Id, storageAccountRepository, resourceGroupRepository, accessService);
        var result = await StorageAccountAccessHelper.GetWithReadAccessAsync(ctx, cancellationToken);

        if (result.IsError)
            return result.Errors;

        return mapper.Map<StorageAccountResult>(result.Value);
    }
}

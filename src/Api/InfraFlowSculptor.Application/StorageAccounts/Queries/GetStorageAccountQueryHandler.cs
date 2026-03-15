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
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<GetStorageAccountQuery, ErrorOr<StorageAccountResult>>
{
    public async Task<ErrorOr<StorageAccountResult>> Handle(GetStorageAccountQuery query, CancellationToken cancellationToken)
    {
        var result = await StorageAccountAccessHelper.GetWithReadAccessAsync(
            query.Id, storageAccountRepository, resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);

        if (result.IsError)
            return result.Errors;

        return mapper.Map<StorageAccountResult>(result.Value);
    }
}

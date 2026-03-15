using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.KeyVaults.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Queries;

public class GetKeyVaultQueryHandler(
    IKeyVaultRepository keyVaultRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<GetKeyVaultQuery, ErrorOr<KeyVaultResult>>
{
    public async Task<ErrorOr<KeyVaultResult>> Handle(GetKeyVaultQuery query, CancellationToken cancellationToken)
    {
        var result = await KeyVaultAccessHelper.GetWithReadAccessAsync(
            query.Id, keyVaultRepository, resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);

        if (result.IsError)
            return result.Errors;

        return mapper.Map<KeyVaultResult>(result.Value);
    }
}
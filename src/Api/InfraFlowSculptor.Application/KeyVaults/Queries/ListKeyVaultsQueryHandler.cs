using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Queries;

public class ListKeyVaultsQueryHandler(
    IKeyVaultRepository keyVaultRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<ListKeyVaultsQuery, List<KeyVaultResult>>
{
    public async Task<ErrorOr<List<KeyVaultResult>>> Handle(ListKeyVaultsQuery query, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(query.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(query.ResourceGroupId);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.ResourceGroup.NotFound(query.ResourceGroupId);

        var keyVaults = await keyVaultRepository.GetByResourceGroupIdAsync(query.ResourceGroupId, cancellationToken);

        return keyVaults.Select(kv => mapper.Map<KeyVaultResult>(kv)).ToList();
    }
}

using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Queries;

public class GetKeyVaultQueryHandler(
    IKeyVaultRepository keyVaultRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetKeyVaultQuery, KeyVaultResult>
{
    public async Task<ErrorOr<KeyVaultResult>> Handle(GetKeyVaultQuery query, CancellationToken cancellationToken)
    {
        var keyVault = await keyVaultRepository.GetByIdAsync(query.Id, cancellationToken);
        if (keyVault is null)
            return Errors.KeyVault.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(keyVault.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.KeyVault.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        return mapper.Map<KeyVaultResult>(keyVault);
    }
}

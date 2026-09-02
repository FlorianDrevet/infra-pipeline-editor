using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.KeyVaults.Queries;

public class GetKeyVaultQueryHandler(
    IKeyVaultRepository keyVaultRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetKeyVaultQuery, KeyVaultResult>
{
    public async Task<ErrorOr<KeyVaultResult>> Handle(GetKeyVaultQuery query, CancellationToken cancellationToken)
    {
        var keyVault = await keyVaultRepository.GetByIdReadOnlyAsync(query.Id, cancellationToken);
        if (keyVault is null)
            return Errors.KeyVault.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(keyVault.ResourceGroup!.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return Errors.KeyVault.NotFoundError(query.Id);

        return mapper.Map<KeyVaultResult>(keyVault);
    }
}

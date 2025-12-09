using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Queries;

public class GetKeyVaultQueryHandler(IKeyVaultRepository keyVaultRepository, IMapper mapper)   
    : IRequestHandler<GetKeyVaultQuery, ErrorOr<KeyVaultResult>>
{
    public async Task<ErrorOr<KeyVaultResult>> Handle(GetKeyVaultQuery query, CancellationToken cancellationToken)
    {
        var keyVault = await keyVaultRepository.GetByIdAsync(query.Id);
        if (keyVault is null)
        {
            return Errors.KeyVault.NotFoundError(query.Id);
        }

        var keyVaultResult = mapper.Map<KeyVaultResult>(keyVault);
        return keyVaultResult;
    }
}
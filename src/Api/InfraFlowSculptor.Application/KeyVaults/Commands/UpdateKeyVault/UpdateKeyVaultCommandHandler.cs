using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.KeyVaults.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.UpdateKeyVault;

public class UpdateKeyVaultCommandHandler(
    IKeyVaultRepository keyVaultRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<UpdateKeyVaultCommand, ErrorOr<KeyVaultResult>>
{
    public async Task<ErrorOr<KeyVaultResult>> Handle(UpdateKeyVaultCommand request, CancellationToken cancellationToken)
    {
        var kvResult = await KeyVaultAccessHelper.GetWithWriteAccessAsync(
            request.Id, keyVaultRepository, resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);

        if (kvResult.IsError)
            return kvResult.Errors;

        kvResult.Value.Update(request.Name, request.Location, request.Sku);

        var updatedKeyVault = await keyVaultRepository.UpdateAsync(kvResult.Value);

        return mapper.Map<KeyVaultResult>(updatedKeyVault);
    }
}

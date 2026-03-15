using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.Errors;
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
        var keyVault = await keyVaultRepository.GetByIdAsync(request.Id, cancellationToken);
        if (keyVault is null)
            return Errors.KeyVault.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(keyVault.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.KeyVault.NotFoundError(request.Id);

        var authResult = await InfraConfigAccessHelper.VerifyWriteAccessAsync(
            infraConfigRepository, currentUser, resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        keyVault.Update(request.Name, request.Location, request.Sku);

        var updatedKeyVault = await keyVaultRepository.UpdateAsync(keyVault);

        return mapper.Map<KeyVaultResult>(updatedKeyVault);
    }
}

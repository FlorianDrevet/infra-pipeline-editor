using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.DeleteKeyVault;

public class DeleteKeyVaultCommandHandler(
    IKeyVaultRepository keyVaultRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DeleteKeyVaultCommand, Deleted>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteKeyVaultCommand request, CancellationToken cancellationToken)
    {
        var keyVault = await keyVaultRepository.GetByIdAsync(request.Id, cancellationToken);
        if (keyVault is null)
            return Errors.KeyVault.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(keyVault.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.KeyVault.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        await keyVaultRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}

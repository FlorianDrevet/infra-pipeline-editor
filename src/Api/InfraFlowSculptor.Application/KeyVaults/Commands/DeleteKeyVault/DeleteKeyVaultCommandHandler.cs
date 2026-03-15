using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.KeyVaults.Common;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.DeleteKeyVault;

public class DeleteKeyVaultCommandHandler(
    IKeyVaultRepository keyVaultRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteKeyVaultCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteKeyVaultCommand request, CancellationToken cancellationToken)
    {
        var authResult = await KeyVaultAccessHelper.GetWithWriteAccessAsync(
            request.Id, keyVaultRepository, resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        await keyVaultRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}

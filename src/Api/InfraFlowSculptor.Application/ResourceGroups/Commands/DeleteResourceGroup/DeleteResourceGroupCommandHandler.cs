using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.ResourceGroups.Commands.DeleteResourceGroup;

public class DeleteResourceGroupCommandHandler(
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DeleteResourceGroupCommand, Deleted>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteResourceGroupCommand request, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.Id, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await resourceGroupRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}

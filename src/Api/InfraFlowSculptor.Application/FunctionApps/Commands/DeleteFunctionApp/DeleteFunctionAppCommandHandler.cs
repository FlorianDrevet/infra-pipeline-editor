using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.FunctionApps.Commands.DeleteFunctionApp;

/// <summary>Handles the <see cref="DeleteFunctionAppCommand"/> request.</summary>
public sealed class DeleteFunctionAppCommandHandler(
    IFunctionAppRepository functionAppRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<DeleteFunctionAppCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteFunctionAppCommand request,
        CancellationToken cancellationToken)
    {
        var functionApp = await functionAppRepository.GetByIdAsync(request.Id, cancellationToken);
        if (functionApp is null)
            return Errors.FunctionApp.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(functionApp.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.FunctionApp.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await functionAppRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}

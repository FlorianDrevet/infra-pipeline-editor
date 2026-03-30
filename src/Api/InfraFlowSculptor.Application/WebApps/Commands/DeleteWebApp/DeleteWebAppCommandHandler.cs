using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.WebApps.Commands.DeleteWebApp;

/// <summary>Handles the <see cref="DeleteWebAppCommand"/> request.</summary>
public class DeleteWebAppCommandHandler(
    IWebAppRepository webAppRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DeleteWebAppCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteWebAppCommand request,
        CancellationToken cancellationToken)
    {
        var webApp = await webAppRepository.GetByIdAsync(request.Id, cancellationToken);
        if (webApp is null)
            return Errors.WebApp.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(webApp.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.WebApp.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await webAppRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}

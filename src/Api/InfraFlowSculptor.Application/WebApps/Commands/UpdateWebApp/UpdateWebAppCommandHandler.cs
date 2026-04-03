using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.WebApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.WebApps.Commands.UpdateWebApp;

/// <summary>Handles the <see cref="UpdateWebAppCommand"/> request.</summary>
public class UpdateWebAppCommandHandler(
    IWebAppRepository webAppRepository,
    IAppServicePlanRepository appServicePlanRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateWebAppCommand, WebAppResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<WebAppResult>> Handle(
        UpdateWebAppCommand request,
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

        // Verify the App Service Plan exists
        var appServicePlanId = new AzureResourceId(request.AppServicePlanId);
        var appServicePlan = await appServicePlanRepository.GetByIdAsync(appServicePlanId, cancellationToken);
        if (appServicePlan is null)
            return Errors.AppServicePlan.NotFoundError(appServicePlanId);

        var runtimeStack = new WebAppRuntimeStack(
            Enum.Parse<WebAppRuntimeStack.WebAppRuntimeStackEnum>(request.RuntimeStack));

        var deploymentMode = new DeploymentMode(
            Enum.Parse<DeploymentMode.DeploymentModeType>(request.DeploymentMode));

        var containerRegistryId = request.ContainerRegistryId.HasValue
            ? new AzureResourceId(request.ContainerRegistryId.Value)
            : (AzureResourceId?)null;

        webApp.Update(request.Name, request.Location, appServicePlanId, runtimeStack, request.RuntimeVersion, request.AlwaysOn, request.HttpsOnly, deploymentMode, containerRegistryId, request.DockerImageName, request.DockerfilePath, request.SourceCodePath, request.BuildCommand);

        if (request.EnvironmentSettings is not null)
            webApp.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName,
                        ec.AlwaysOn,
                        ec.HttpsOnly,
                        ec.DockerImageTag))
                    .ToList());

        var updated = await webAppRepository.UpdateAsync(webApp);

        return mapper.Map<WebAppResult>(updated);
    }
}

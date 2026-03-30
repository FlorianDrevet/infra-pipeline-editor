using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.WebApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.WebApps.Commands.CreateWebApp;

/// <summary>Handles the <see cref="CreateWebAppCommand"/> request.</summary>
public class CreateWebAppCommandHandler(
    IWebAppRepository webAppRepository,
    IAppServicePlanRepository appServicePlanRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateWebAppCommand, WebAppResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<WebAppResult>> Handle(
        CreateWebAppCommand request,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

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

        var webApp = WebApp.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            appServicePlanId,
            runtimeStack,
            request.RuntimeVersion,
            request.AlwaysOn,
            request.HttpsOnly,
            deploymentMode,
            containerRegistryId,
            request.DockerImageName,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName,
                    ec.AlwaysOn,
                    ec.HttpsOnly,
                    ec.RuntimeStack is not null
                        ? new WebAppRuntimeStack(Enum.Parse<WebAppRuntimeStack.WebAppRuntimeStackEnum>(ec.RuntimeStack))
                        : (WebAppRuntimeStack?)null,
                    ec.RuntimeVersion,
                    ec.DockerImageTag))
                .ToList());

        var saved = await webAppRepository.AddAsync(webApp);

        return mapper.Map<WebAppResult>(saved);
    }
}

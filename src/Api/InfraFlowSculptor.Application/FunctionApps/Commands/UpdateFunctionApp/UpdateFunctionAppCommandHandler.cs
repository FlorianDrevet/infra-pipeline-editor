using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.FunctionApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;
using MapsterMapper;

namespace InfraFlowSculptor.Application.FunctionApps.Commands.UpdateFunctionApp;

/// <summary>Handles the <see cref="UpdateFunctionAppCommand"/> request.</summary>
public sealed class UpdateFunctionAppCommandHandler(
    IFunctionAppRepository functionAppRepository,
    IAppServicePlanRepository appServicePlanRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateFunctionAppCommand, FunctionAppResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<FunctionAppResult>> Handle(
        UpdateFunctionAppCommand request,
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

        // Verify the App Service Plan exists
        var appServicePlanId = new AzureResourceId(request.AppServicePlanId);
        var appServicePlan = await appServicePlanRepository.GetByIdAsync(appServicePlanId, cancellationToken);
        if (appServicePlan is null)
            return Errors.AppServicePlan.NotFoundError(appServicePlanId);

        var runtimeStack = new FunctionAppRuntimeStack(
            Enum.Parse<FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum>(request.RuntimeStack));

        var deploymentMode = new DeploymentMode(
            Enum.Parse<DeploymentMode.DeploymentModeType>(request.DeploymentMode));

        var containerRegistryId = request.ContainerRegistryId.HasValue
            ? new AzureResourceId(request.ContainerRegistryId.Value)
            : (AzureResourceId?)null;

        functionApp.Update(request.Name, request.Location, appServicePlanId, runtimeStack, request.RuntimeVersion, request.HttpsOnly, deploymentMode, containerRegistryId,
            !string.IsNullOrWhiteSpace(request.AcrAuthMode)
                ? new AcrAuthMode(Enum.Parse<AcrAuthMode.AcrAuthModeType>(request.AcrAuthMode))
                : null,
            request.AcrPullIdentityId.HasValue
                ? new AzureResourceId(request.AcrPullIdentityId.Value)
                : null,
            request.DockerImageName, request.DockerImageValidated, request.DockerfilePath, request.SourceCodePath, request.BuildCommand, request.ApplicationName);

        if (request.EnvironmentSettings is not null)
            functionApp.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName,
                        ec.HttpsOnly,
                        ec.MaxInstanceCount,
                        ec.DockerImageTag))
                    .ToList());

        if (request.PipelineStepOptions is { } opts)
        {
            functionApp.PipelineStepOptions.Update(PipelineStepOptionsDataMapper.ToDomainData(opts));
        }

        var updated = functionAppRepository.Update(functionApp);

        return mapper.Map<FunctionAppResult>(updated);
    }
}

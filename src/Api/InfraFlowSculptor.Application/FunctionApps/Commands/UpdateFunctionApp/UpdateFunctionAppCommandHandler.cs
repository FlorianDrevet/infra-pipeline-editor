using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.FunctionApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

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

        functionApp.Update(request.Name, request.Location, appServicePlanId, runtimeStack, request.RuntimeVersion, request.HttpsOnly, deploymentMode, containerRegistryId, request.DockerImageName, request.DockerfilePath, request.SourceCodePath, request.BuildCommand);

        if (request.EnvironmentSettings is not null)
            functionApp.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName,
                        ec.HttpsOnly,
                        ec.MaxInstanceCount,
                        ec.DockerImageTag))
                    .ToList());

        var updated = await functionAppRepository.UpdateAsync(functionApp);

        return mapper.Map<FunctionAppResult>(updated);
    }
}

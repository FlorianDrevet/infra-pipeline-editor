using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.FunctionApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.FunctionApps.Commands.CreateFunctionApp;

/// <summary>Handles the <see cref="CreateFunctionAppCommand"/> request.</summary>
public sealed class CreateFunctionAppCommandHandler(
    IFunctionAppRepository functionAppRepository,
    IAppServicePlanRepository appServicePlanRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateFunctionAppCommand, FunctionAppResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<FunctionAppResult>> Handle(
        CreateFunctionAppCommand request,
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

        var runtimeStack = new FunctionAppRuntimeStack(
            Enum.Parse<FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum>(request.RuntimeStack));

        var deploymentMode = new DeploymentMode(
            Enum.Parse<DeploymentMode.DeploymentModeType>(request.DeploymentMode));

        var containerRegistryId = request.ContainerRegistryId.HasValue
            ? new AzureResourceId(request.ContainerRegistryId.Value)
            : (AzureResourceId?)null;

        var functionApp = FunctionApp.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            appServicePlanId,
            runtimeStack,
            request.RuntimeVersion,
            request.HttpsOnly,
            deploymentMode,
            containerRegistryId,
            request.DockerImageName,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName,
                    ec.HttpsOnly,
                    ec.MaxInstanceCount,
                    ec.FunctionsWorkerRuntime,
                    ec.DockerImageTag))
                .ToList());

        var saved = await functionAppRepository.AddAsync(functionApp);

        return mapper.Map<FunctionAppResult>(saved);
    }
}

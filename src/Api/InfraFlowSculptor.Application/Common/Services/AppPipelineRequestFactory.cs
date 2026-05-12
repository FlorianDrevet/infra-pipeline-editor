using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.Application.Common.Services;

/// <summary>
/// Creates typed app pipeline requests by loading the concrete compute resource and its optional container registry.
/// </summary>
public sealed class AppPipelineRequestFactory(
    IContainerAppRepository containerAppRepository,
    IWebAppRepository webAppRepository,
    IFunctionAppRepository functionAppRepository,
    IContainerRegistryRepository containerRegistryRepository)
    : IAppPipelineRequestFactory
{
    /// <inheritdoc />
    public async Task<AppPipelineGenerationRequest?> CreateAsync(
        AzureResourceId resourceId,
        string resourceType,
        CancellationToken cancellationToken = default)
    {
        return resourceType switch
        {
            AzureResourceTypes.ArmTypes.ContainerAppType => await CreateFromContainerAppAsync(resourceId, cancellationToken)
                .ConfigureAwait(false),
            AzureResourceTypes.ArmTypes.WebAppType => await CreateFromWebAppAsync(resourceId, cancellationToken)
                .ConfigureAwait(false),
            AzureResourceTypes.ArmTypes.FunctionAppType => await CreateFromFunctionAppAsync(resourceId, cancellationToken)
                .ConfigureAwait(false),
            _ => null,
        };
    }

    private async Task<AppPipelineGenerationRequest?> CreateFromContainerAppAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken)
    {
        var containerApp = await containerAppRepository.GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false);
        if (containerApp is null)
            return null;

        var containerRegistryName = await ResolveContainerRegistryNameAsync(
            containerApp.ContainerRegistryId,
            cancellationToken).ConfigureAwait(false);

        return new AppPipelineGenerationRequest
        {
            ResourceName = containerApp.Name,
            ApplicationName = containerApp.ApplicationName,
            ResourceType = AzureResourceTypes.ContainerApp,
            DeploymentMode = DeploymentMode.DeploymentModeType.Container.ToString(),
            DockerfilePath = containerApp.DockerfilePath,
            DockerImageName = containerApp.DockerImageName,
            ContainerRegistryName = containerRegistryName,
            AcrAuthMode = containerApp.AcrAuthMode?.Value.ToString(),
            PromotionStrategy = AppPipelinePromotionStrategy.AcrImport,
            EnableSecurityScans = true,
        };
    }

    private async Task<AppPipelineGenerationRequest?> CreateFromWebAppAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken)
    {
        var webApp = await webAppRepository.GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false);
        if (webApp is null)
            return null;

        var containerRegistryName = await ResolveContainerRegistryNameAsync(
            webApp.ContainerRegistryId,
            cancellationToken).ConfigureAwait(false);

        return new AppPipelineGenerationRequest
        {
            ResourceName = webApp.Name,
            ApplicationName = webApp.ApplicationName,
            ResourceType = AzureResourceTypes.WebApp,
            DeploymentMode = webApp.DeploymentMode.Value.ToString(),
            DockerfilePath = webApp.DockerfilePath,
            SourceCodePath = webApp.SourceCodePath,
            BuildCommand = webApp.BuildCommand,
            DockerImageName = webApp.DockerImageName,
            ContainerRegistryName = containerRegistryName,
            AcrAuthMode = webApp.AcrAuthMode?.Value.ToString(),
            RuntimeStack = webApp.RuntimeStack.Value.ToString(),
            RuntimeVersion = webApp.RuntimeVersion,
            PromotionStrategy = AppPipelinePromotionStrategy.AcrImport,
            EnableSecurityScans = true,
        };
    }

    private async Task<AppPipelineGenerationRequest?> CreateFromFunctionAppAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken)
    {
        var functionApp = await functionAppRepository.GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false);
        if (functionApp is null)
            return null;

        var containerRegistryName = await ResolveContainerRegistryNameAsync(
            functionApp.ContainerRegistryId,
            cancellationToken).ConfigureAwait(false);

        return new AppPipelineGenerationRequest
        {
            ResourceName = functionApp.Name,
            ApplicationName = functionApp.ApplicationName,
            ResourceType = AzureResourceTypes.FunctionApp,
            DeploymentMode = functionApp.DeploymentMode.Value.ToString(),
            DockerfilePath = functionApp.DockerfilePath,
            SourceCodePath = functionApp.SourceCodePath,
            BuildCommand = functionApp.BuildCommand,
            DockerImageName = functionApp.DockerImageName,
            ContainerRegistryName = containerRegistryName,
            AcrAuthMode = functionApp.AcrAuthMode?.Value.ToString(),
            RuntimeStack = functionApp.RuntimeStack.Value.ToString(),
            RuntimeVersion = functionApp.RuntimeVersion,
            PromotionStrategy = AppPipelinePromotionStrategy.AcrImport,
            EnableSecurityScans = true,
        };
    }

    private async Task<string?> ResolveContainerRegistryNameAsync(
        AzureResourceId? containerRegistryId,
        CancellationToken cancellationToken)
    {
        if (containerRegistryId is null)
            return null;

        var containerRegistry = await containerRegistryRepository.GetByIdAsync(containerRegistryId, cancellationToken)
            .ConfigureAwait(false);

        return containerRegistry?.Name.Value;
    }
}
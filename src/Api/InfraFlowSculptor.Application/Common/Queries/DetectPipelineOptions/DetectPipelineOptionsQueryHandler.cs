using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

namespace InfraFlowSculptor.Application.Common.Queries.DetectPipelineOptions;

/// <summary>
/// Handles <see cref="DetectPipelineOptionsQuery"/> by loading the compute resource,
/// resolving its project repository, and invoking auto-detection.
/// </summary>
public sealed class DetectPipelineOptionsQueryHandler(
    IWebAppRepository webAppRepository,
    IFunctionAppRepository functionAppRepository,
    IContainerAppRepository containerAppRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    IProjectRepository projectRepository,
    IKeyVaultSecretClient keyVaultClient,
    IGitProviderFactory gitProviderFactory,
    IRepositoryTargetResolver repositoryTargetResolver,
    IPipelineOptionDetectionService detectionService)
    : IQueryHandler<DetectPipelineOptionsQuery, DetectedPipelineOptionsResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<DetectedPipelineOptionsResult>> Handle(
        DetectPipelineOptionsQuery query, CancellationToken cancellationToken)
    {
        // 1. Resolve the compute resource and extract runtime/source info
        var resourceInfo = await ResolveComputeResourceInfoAsync(query.ResourceId, cancellationToken);
        if (resourceInfo.IsError)
            return resourceInfo.Errors;

        var (runtimeStack, sourceCodePath, resourceGroupId) = resourceInfo.Value;

        // 2. Navigate ResourceGroup → InfraConfig → Project
        var resourceGroup = await resourceGroupRepository.GetByIdReadOnlyAsync(resourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(resourceGroupId);

        var infraConfig = await infraConfigRepository.GetByIdReadOnlyAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(resourceGroup.InfraConfigId);

        var project = await projectRepository.GetByIdWithAllAsync(infraConfig.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(infraConfig.ProjectId);

        // 3. Resolve the source code repository target
        var targetResult = repositoryTargetResolver.Resolve(project, infraConfig, ArtifactKind.Pipeline);
        if (targetResult.IsError)
            return targetResult.Errors;

        var target = targetResult.Value;

        // 4. Retrieve the PAT
        var secretName = target.PatSecretName ?? $"git-pat-{project.Id.Value}";
        var secretResult = await keyVaultClient.GetSecretAsync(secretName, cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        // 5. Run detection
        var gitProvider = gitProviderFactory.Create(target.ProviderType);
        return await detectionService.DetectAsync(
            gitProvider,
            secretResult.Value,
            target.Owner,
            target.RepositoryName,
            target.Branch,
            runtimeStack,
            sourceCodePath,
            cancellationToken);
    }

    private async Task<ErrorOr<(string RuntimeStack, string? SourceCodePath, Domain.ResourceGroupAggregate.ValueObjects.ResourceGroupId ResourceGroupId)>> ResolveComputeResourceInfoAsync(
        Domain.Common.BaseModels.ValueObjects.AzureResourceId resourceId,
        CancellationToken cancellationToken)
    {
        // Try WebApp first
        var webApp = await webAppRepository.GetByIdReadOnlyAsync(resourceId, cancellationToken);
        if (webApp is not null)
        {
            return (webApp.RuntimeStack.Value.ToString(), webApp.SourceCodePath, webApp.ResourceGroupId);
        }

        // Try FunctionApp
        var functionApp = await functionAppRepository.GetByIdReadOnlyAsync(resourceId, cancellationToken);
        if (functionApp is not null)
        {
            return (functionApp.RuntimeStack.Value.ToString(), functionApp.SourceCodePath, functionApp.ResourceGroupId);
        }

        // Try ContainerApp (uses PipelineStepOptions.Stack instead of a direct RuntimeStack property)
        var containerApp = await containerAppRepository.GetByIdReadOnlyAsync(resourceId, cancellationToken);
        if (containerApp is not null)
        {
            var stack = containerApp.PipelineStepOptions.Stack.Value == ApplicationStack.ApplicationStackEnum.Unknown
                ? "DotNet"
                : containerApp.PipelineStepOptions.Stack.Value.ToString();
            return (stack, containerApp.SourceCodePath, containerApp.ResourceGroupId);
        }

        return Errors.AzureResource.NotFound(resourceId);
    }
}

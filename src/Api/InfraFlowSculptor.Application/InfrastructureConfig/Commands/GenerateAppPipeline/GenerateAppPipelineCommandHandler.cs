using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateAppPipeline;

/// <summary>
/// Handles the <see cref="GenerateAppPipelineCommand"/> by resolving the target compute resource,
/// building an <see cref="AppPipelineGenerationRequest"/>, generating pipeline YAML,
/// and uploading the resulting artifacts to blob storage.
/// </summary>
public sealed class GenerateAppPipelineCommandHandler(
    IInfrastructureConfigReadRepository configRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    IProjectRepository projectRepository,
    IContainerAppRepository containerAppRepository,
    IWebAppRepository webAppRepository,
    IFunctionAppRepository functionAppRepository,
    IContainerRegistryRepository containerRegistryRepository,
    AppPipelineGenerationEngine appPipelineGenerationEngine,
    IGeneratedArtifactService artifactService)
    : ICommandHandler<GenerateAppPipelineCommand, GenerateAppPipelineResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<GenerateAppPipelineResult>> Handle(
        GenerateAppPipelineCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Load the infrastructure config with resources (for environments, project reference, variable groups)
        var config = await configRepository.GetByIdWithResourcesAsync(
            command.InfrastructureConfigId, cancellationToken).ConfigureAwait(false);

        if (config is null)
            return Errors.InfrastructureConfig.NotFoundError(new InfrastructureConfigId(command.InfrastructureConfigId));

        // 2. Verify the resource exists in this config
        var resourceId = new AzureResourceId(command.ResourceId);
        var resourceReadModel = config.ResourceGroups
            .SelectMany(rg => rg.Resources)
            .FirstOrDefault(r => r.Id == command.ResourceId);

        if (resourceReadModel is null)
            return Error.NotFound(
                code: "AzureResource.NotFound",
                description: $"No resource with id {command.ResourceId} was found in this infrastructure configuration.");

        // 3. Resolve the typed compute resource and build the generation request
        var requestOrError = await ResolveComputeResourceAsync(
            resourceId, resourceReadModel.ResourceType, cancellationToken).ConfigureAwait(false);

        if (requestOrError.IsError)
            return requestOrError.Errors;

        var generationRequest = requestOrError.Value;

        // 4. Populate shared fields from the infrastructure config
        generationRequest.ConfigName = config.Name;

        generationRequest.Environments = config.Environments
            .Select(e => new EnvironmentDefinition
            {
                Name = e.Name,
                ShortName = e.ShortName,
                Location = e.Location,
                Prefix = e.Prefix,
                Suffix = e.Suffix,
                AzureResourceManagerConnection = e.AzureResourceManagerConnection,
                SubscriptionId = e.SubscriptionId,
                Tags = e.Tags,
            })
            .ToList();

        // 5. Load project-level pipeline variable groups (same pattern as GeneratePipelineCommandHandler)
        var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
            new ProjectId(config.ProjectId), cancellationToken).ConfigureAwait(false);

        generationRequest.PipelineVariableGroups = project?.PipelineVariableGroups
            .Select(g =>
            {
                var mappings = config.AppSettings
                    .Where(s => s.IsViaVariableGroup && s.VariableGroupId.HasValue
                        && s.VariableGroupId.Value == g.Id.Value)
                    .Select(s => new PipelineVariableMappingDefinition
                    {
                        PipelineVariableName = s.PipelineVariableName!,
                        BicepParameterName = s.Name,
                    })
                    .ToList();

                return new PipelineVariableGroupDefinition
                {
                    GroupName = g.GroupName,
                    Mappings = mappings,
                };
            }).ToList() ?? [];

        // 6. Determine mono-repo: project has multiple configs
        var allConfigs = await infraConfigRepository
            .GetByProjectIdAsync(new ProjectId(config.ProjectId), cancellationToken).ConfigureAwait(false);
        generationRequest.IsMonoRepo = allConfigs.Count > 1;

        // 7. Generate application pipeline
        var result = appPipelineGenerationEngine.Generate(generationRequest);

        // 8. Upload artifacts
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var fileUris = new Dictionary<string, Uri>();

        foreach (var (path, content) in result.Files)
        {
            var uri = await artifactService.UploadArtifactAsync(
                "app-pipeline", command.InfrastructureConfigId, timestamp, path, content).ConfigureAwait(false);
            fileUris[path] = uri;
        }

        return new GenerateAppPipelineResult(fileUris);
    }

    /// <summary>
    /// Resolves the typed compute resource (ContainerApp, WebApp, or FunctionApp) by its identifier
    /// and builds an <see cref="AppPipelineGenerationRequest"/> from its properties.
    /// </summary>
    private async Task<ErrorOr<AppPipelineGenerationRequest>> ResolveComputeResourceAsync(
        AzureResourceId resourceId,
        string resourceType,
        CancellationToken cancellationToken)
    {
        return resourceType switch
        {
            AzureResourceTypes.ContainerApp => await BuildFromContainerAppAsync(resourceId, cancellationToken)
                .ConfigureAwait(false),
            AzureResourceTypes.WebApp => await BuildFromWebAppAsync(resourceId, cancellationToken)
                .ConfigureAwait(false),
            AzureResourceTypes.FunctionApp => await BuildFromFunctionAppAsync(resourceId, cancellationToken)
                .ConfigureAwait(false),
            _ => Error.Validation(
                code: "AzureResource.NotComputeType",
                description: $"Resource type '{resourceType}' is not a supported compute type for application pipeline generation. Supported types: ContainerApp, WebApp, FunctionApp."),
        };
    }

    /// <summary>
    /// Builds an <see cref="AppPipelineGenerationRequest"/> from a <see cref="ContainerApp"/> aggregate.
    /// </summary>
    private async Task<ErrorOr<AppPipelineGenerationRequest>> BuildFromContainerAppAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken)
    {
        var containerApp = await containerAppRepository
            .GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false);

        if (containerApp is null)
            return Errors.ContainerApp.NotFoundError(resourceId);

        var containerRegistryName = await ResolveContainerRegistryNameAsync(
            containerApp.ContainerRegistryId, cancellationToken).ConfigureAwait(false);

        return new AppPipelineGenerationRequest
        {
            ResourceName = containerApp.Name,
            ResourceType = AzureResourceTypes.ContainerApp,
            DeploymentMode = DeploymentMode.DeploymentModeType.Container.ToString(),
            DockerfilePath = containerApp.DockerfilePath,
            DockerImageName = containerApp.DockerImageName,
            ContainerRegistryName = containerRegistryName,
        };
    }

    /// <summary>
    /// Builds an <see cref="AppPipelineGenerationRequest"/> from a <see cref="WebApp"/> aggregate.
    /// </summary>
    private async Task<ErrorOr<AppPipelineGenerationRequest>> BuildFromWebAppAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken)
    {
        var webApp = await webAppRepository
            .GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false);

        if (webApp is null)
            return Errors.WebApp.NotFoundError(resourceId);

        var containerRegistryName = await ResolveContainerRegistryNameAsync(
            webApp.ContainerRegistryId, cancellationToken).ConfigureAwait(false);

        return new AppPipelineGenerationRequest
        {
            ResourceName = webApp.Name,
            ResourceType = AzureResourceTypes.WebApp,
            DeploymentMode = webApp.DeploymentMode.Value.ToString(),
            DockerfilePath = webApp.DockerfilePath,
            SourceCodePath = webApp.SourceCodePath,
            BuildCommand = webApp.BuildCommand,
            DockerImageName = webApp.DockerImageName,
            ContainerRegistryName = containerRegistryName,
            RuntimeStack = webApp.RuntimeStack.Value.ToString(),
            RuntimeVersion = webApp.RuntimeVersion,
        };
    }

    /// <summary>
    /// Builds an <see cref="AppPipelineGenerationRequest"/> from a <see cref="FunctionApp"/> aggregate.
    /// </summary>
    private async Task<ErrorOr<AppPipelineGenerationRequest>> BuildFromFunctionAppAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken)
    {
        var functionApp = await functionAppRepository
            .GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false);

        if (functionApp is null)
            return Errors.FunctionApp.NotFoundError(resourceId);

        var containerRegistryName = await ResolveContainerRegistryNameAsync(
            functionApp.ContainerRegistryId, cancellationToken).ConfigureAwait(false);

        return new AppPipelineGenerationRequest
        {
            ResourceName = functionApp.Name,
            ResourceType = AzureResourceTypes.FunctionApp,
            DeploymentMode = functionApp.DeploymentMode.Value.ToString(),
            DockerfilePath = functionApp.DockerfilePath,
            SourceCodePath = functionApp.SourceCodePath,
            BuildCommand = functionApp.BuildCommand,
            DockerImageName = functionApp.DockerImageName,
            ContainerRegistryName = containerRegistryName,
            RuntimeStack = functionApp.RuntimeStack.Value.ToString(),
            RuntimeVersion = functionApp.RuntimeVersion,
        };
    }

    /// <summary>
    /// Resolves the Container Registry name from its identifier, or returns <c>null</c>
    /// if no identifier is provided.
    /// </summary>
    private async Task<string?> ResolveContainerRegistryNameAsync(
        AzureResourceId? containerRegistryId,
        CancellationToken cancellationToken)
    {
        if (containerRegistryId is null)
            return null;

        var containerRegistry = await containerRegistryRepository
            .GetByIdAsync(containerRegistryId, cancellationToken).ConfigureAwait(false);

        return containerRegistry is null ? null : containerRegistry.Name.Value;
    }
}

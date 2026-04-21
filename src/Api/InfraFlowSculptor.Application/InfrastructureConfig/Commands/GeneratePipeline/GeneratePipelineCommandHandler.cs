using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GeneratePipeline;

/// <summary>Handles the <see cref="GeneratePipelineCommand"/>.</summary>
public sealed class GeneratePipelineCommandHandler(
    IInfrastructureConfigReadRepository configRepository,
    IProjectRepository projectRepository,
    PipelineGenerationEngine pipelineGenerationEngine,
    AppPipelineGenerationEngine appPipelineGenerationEngine,
    IContainerAppRepository containerAppRepository,
    IWebAppRepository webAppRepository,
    IFunctionAppRepository functionAppRepository,
    IContainerRegistryRepository containerRegistryRepository,
    IGeneratedArtifactService artifactService)
    : ICommandHandler<GeneratePipelineCommand, GeneratePipelineResult>
{
    public async Task<ErrorOr<GeneratePipelineResult>> Handle(
        GeneratePipelineCommand command,
        CancellationToken cancellationToken)
    {
        var config = await configRepository.GetByIdWithResourcesAsync(
            command.InfrastructureConfigId, cancellationToken);

        if (config is null)
            return Errors.InfrastructureConfig.NotFoundError(new InfrastructureConfigId(command.InfrastructureConfigId));

        var mergedAbbreviations = MergeAbbreviations(config.NamingContext.ResourceAbbreviations);

        // Load project-level pipeline variable groups
        var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
            new ProjectId(config.ProjectId), cancellationToken);
        var projectWithGit = await projectRepository.GetByIdWithAllAsync(
            new ProjectId(config.ProjectId), cancellationToken);

        var projectVariableGroups = project?.PipelineVariableGroups
            .Select(g =>
            {
                // Derive mappings from app settings linked to this variable group
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

        var resources = config.ResourceGroups
            .SelectMany(rg => rg.Resources.Select(r => new ResourceDefinition
            {
                Name = r.Name,
                Type = r.ResourceType,
                ResourceGroupName = rg.Name,
                Sku = r.Properties.GetValueOrDefault("sku", string.Empty),
                Properties = r.Properties,
                ResourceAbbreviation = GetResourceAbbreviation(r.ResourceType, mergedAbbreviations),
                EnvironmentConfigs = r.EnvironmentConfigs
                    .ToDictionary(
                        ec => ec.EnvironmentName,
                        ec => (IReadOnlyDictionary<string, string>)ec.Properties),
                AssignedUserAssignedIdentityName = r.AssignedUserAssignedIdentityName,
            }))
            .ToList();

        var resourceGroups = config.ResourceGroups
            .Select(rg => new ResourceGroupDefinition
            {
                Name = rg.Name,
                Location = rg.Location,
                ResourceAbbreviation = "rg"
            })
            .ToList();

        var environmentNames = config.Environments.Select(e => e.Name).ToList();

        var environments = config.Environments
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

        var namingContext = new NamingContext
        {
            DefaultTemplate = config.NamingContext.DefaultTemplate,
            ResourceTemplates = config.NamingContext.ResourceTemplates,
            ResourceAbbreviations = mergedAbbreviations,
        };

        var generationRequest = new GenerationRequest
        {
            Resources = resources,
            ResourceGroups = resourceGroups,
            Environments = environments,
            EnvironmentNames = environmentNames,
            NamingContext = namingContext,
            RoleAssignments = [],
            AppSettings = [],
            ExistingResourceReferences = [],
            PipelineVariableGroups = projectVariableGroups,
            AgentPoolName = project?.AgentPoolName,
            BicepBasePath = projectWithGit?.GitRepositoryConfiguration?.BasePath,
        };

        var result = pipelineGenerationEngine.Generate(generationRequest, config.Name);

        // ─── App Pipeline Generation ────────────────────────────────────────
        var computeTypes = new HashSet<string>
        {
            AzureResourceTypes.ArmTypes.ContainerApp,
            AzureResourceTypes.ArmTypes.WebApp,
            AzureResourceTypes.ArmTypes.FunctionApp,
        };

        var computeResources = config.ResourceGroups
            .SelectMany(rg => rg.Resources)
            .Where(r => computeTypes.Contains(r.ResourceType))
            .ToList();

        var appRequests = new List<AppPipelineGenerationRequest>();

        foreach (var resource in computeResources)
        {
            var resourceId = new AzureResourceId(resource.Id);
            var req = await BuildAppPipelineRequestAsync(
                resourceId, resource.ResourceType, cancellationToken).ConfigureAwait(false);

            if (req is null)
                continue;

            req.ConfigName = config.Name;
            req.Environments = environments;
            req.PipelineVariableGroups = projectVariableGroups;
            req.AgentPoolName = project?.AgentPoolName;

            appRequests.Add(req);
        }

        var appPipelineMode = Enum.TryParse<AppPipelineMode>(config.AppPipelineMode, out var parsedMode)
            ? parsedMode
            : AppPipelineMode.Isolated;

        var appResult = appPipelineGenerationEngine.GenerateAll(appRequests, appPipelineMode, config.Name);

        // ─── Upload all artifacts ───────────────────────────────────────────
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var fileUris = new Dictionary<string, Uri>();

        foreach (var (path, content) in result.Files)
        {
            var uri = await artifactService.UploadArtifactAsync(
                "pipeline", command.InfrastructureConfigId, timestamp, path, content);
            fileUris[path] = uri;
        }

        foreach (var (path, content) in appResult.Files)
        {
            var uri = await artifactService.UploadArtifactAsync(
                "pipeline", command.InfrastructureConfigId, timestamp, path, content);
            fileUris[path] = uri;
        }

        return new GeneratePipelineResult(fileUris);
    }

    /// <summary>
    /// Builds an <see cref="AppPipelineGenerationRequest"/> from a typed compute resource.
    /// Returns <c>null</c> if the resource is not found.
    /// </summary>
    private async Task<AppPipelineGenerationRequest?> BuildAppPipelineRequestAsync(
        AzureResourceId resourceId,
        string resourceType,
        CancellationToken cancellationToken)
    {
        return resourceType switch
        {
            AzureResourceTypes.ArmTypes.ContainerApp => await BuildFromContainerAppAsync(resourceId, cancellationToken)
                .ConfigureAwait(false),
            AzureResourceTypes.ArmTypes.WebApp => await BuildFromWebAppAsync(resourceId, cancellationToken)
                .ConfigureAwait(false),
            AzureResourceTypes.ArmTypes.FunctionApp => await BuildFromFunctionAppAsync(resourceId, cancellationToken)
                .ConfigureAwait(false),
            _ => null,
        };
    }

    private async Task<AppPipelineGenerationRequest?> BuildFromContainerAppAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken)
    {
        var containerApp = await containerAppRepository
            .GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false);

        if (containerApp is null)
            return null;

        var containerRegistryName = await ResolveContainerRegistryNameAsync(
            containerApp.ContainerRegistryId, cancellationToken).ConfigureAwait(false);

        return new AppPipelineGenerationRequest
        {
            ResourceName = containerApp.Name,
            ApplicationName = containerApp.ApplicationName,
            ResourceType = AzureResourceTypes.ContainerApp,
            DeploymentMode = DeploymentMode.DeploymentModeType.Container.ToString(),
            DockerfilePath = containerApp.DockerfilePath,
            DockerImageName = containerApp.DockerImageName,
            ContainerRegistryName = containerRegistryName,
        };
    }

    private async Task<AppPipelineGenerationRequest?> BuildFromWebAppAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken)
    {
        var webApp = await webAppRepository
            .GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false);

        if (webApp is null)
            return null;

        var containerRegistryName = await ResolveContainerRegistryNameAsync(
            webApp.ContainerRegistryId, cancellationToken).ConfigureAwait(false);

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
            RuntimeStack = webApp.RuntimeStack.Value.ToString(),
            RuntimeVersion = webApp.RuntimeVersion,
        };
    }

    private async Task<AppPipelineGenerationRequest?> BuildFromFunctionAppAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken)
    {
        var functionApp = await functionAppRepository
            .GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false);

        if (functionApp is null)
            return null;

        var containerRegistryName = await ResolveContainerRegistryNameAsync(
            functionApp.ContainerRegistryId, cancellationToken).ConfigureAwait(false);

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
            RuntimeStack = functionApp.RuntimeStack.Value.ToString(),
            RuntimeVersion = functionApp.RuntimeVersion,
        };
    }

    private async Task<string?> ResolveContainerRegistryNameAsync(
        AzureResourceId? containerRegistryId,
        CancellationToken cancellationToken)
    {
        if (containerRegistryId is null)
            return null;

        var containerRegistry = await containerRegistryRepository
            .GetByIdAsync(containerRegistryId, cancellationToken).ConfigureAwait(false);

        return containerRegistry?.Name.Value;
    }

    private static string GetResourceTypeName(string azureResourceType) =>
        AzureResourceTypes.GetFriendlyName(azureResourceType);

    /// <summary>
    /// Resolves the resource abbreviation from the Azure resource type string,
    /// preferring overrides from the merged abbreviation dictionary.
    /// </summary>
    private static string GetResourceAbbreviation(
        string azureResourceType,
        IReadOnlyDictionary<string, string> mergedAbbreviations)
    {
        var typeName = AzureResourceTypes.GetFriendlyName(azureResourceType);
        return mergedAbbreviations.TryGetValue(typeName, out var abbr)
            ? abbr
            : ResourceAbbreviationCatalog.GetAbbreviation(typeName);
    }

    /// <summary>
    /// Merges the catalog defaults with user overrides.
    /// Overrides take precedence over catalog entries.
    /// </summary>
    private static IReadOnlyDictionary<string, string> MergeAbbreviations(
        IReadOnlyDictionary<string, string> overrides)
    {
        var merged = new Dictionary<string, string>(ResourceAbbreviationCatalog.GetAll(), StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in overrides)
        {
            merged[key] = value;
        }

        return merged;
    }
}

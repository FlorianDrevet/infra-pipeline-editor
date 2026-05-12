using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration;
using MediatR;
using AppPipelineMode = InfraFlowSculptor.GenerationCore.Models.AppPipelineMode;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GeneratePipeline;

/// <summary>Handles the <see cref="GeneratePipelineCommand"/>.</summary>
public sealed class GeneratePipelineCommandHandler(
    IInfrastructureConfigReadRepository configRepository,
    IProjectRepository projectRepository,
    PipelineGenerationEngine pipelineGenerationEngine,
    AppPipelineGenerationEngine appPipelineGenerationEngine,
    IAppPipelineRequestFactory appPipelineRequestFactory,
    IEnumerable<IResourceTypeBicepSpecGenerator> bicepGenerators,
    IGeneratedArtifactService artifactService,
    IRepositoryTargetResolver targetResolver,
    IInfrastructureConfigRepository infraConfigRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<GeneratePipelineCommand, GeneratePipelineResult>
{
    public async Task<ErrorOr<GeneratePipelineResult>> Handle(
        GeneratePipelineCommand command,
        CancellationToken cancellationToken)
    {
        var configId = new InfrastructureConfigId(command.InfrastructureConfigId);

        var accessResult = await accessService.VerifyWriteAccessAsync(configId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var config = await configRepository.GetByIdWithResourcesAsync(
            command.InfrastructureConfigId, cancellationToken);

        if (config is null)
            return Errors.InfrastructureConfig.NotFoundError(configId);

        var mergedAbbreviations = MergeAbbreviations(config.NamingContext.ResourceAbbreviations);

        // Load project-level pipeline variable groups
        var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
            new ProjectId(config.ProjectId), cancellationToken);
        // Load project (with Repositories + legacy GitRepositoryConfiguration) for V2 routing.
        var projectWithGit = await projectRepository.GetByIdWithAllAsync(
            new ProjectId(config.ProjectId), cancellationToken);

        // Load the domain InfrastructureConfig entity so the resolver can honor its RepositoryBinding.
        var domainConfig = await infraConfigRepository.GetByIdAsync(
            new InfrastructureConfigId(command.InfrastructureConfigId), cancellationToken);

        // Resolve the pipeline-kind target to derive the BicepBasePath used by release pipeline YAML
        // (infra path inside the target repo). A missing repository is tolerated here: BicepBasePath
        // simply becomes null, matching the previous behavior when no Git configuration existed.
        string? bicepBasePath = null;
        if (projectWithGit is not null && domainConfig is not null)
        {
            var targetResult = targetResolver.Resolve(projectWithGit, domainConfig, ArtifactKind.Pipeline);
            if (!targetResult.IsError)
            {
                bicepBasePath = targetResult.Value.BasePath;
            }
        }

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
                        BicepParameterName = AppSettingPipelineParameterNameHelper.ResolveBicepParameterName(s),
                    })
                    .ToList();

                return new PipelineVariableGroupDefinition
                {
                    GroupName = g.GroupName,
                    Mappings = mappings,
                };
            }).ToList() ?? [];

        var resources = config.ResourceGroups
            .SelectMany(rg => rg.Resources
                .Where(r => !r.IsExisting)
                .Select(r => new ResourceDefinition
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
            SecureParameterOverrides = SecureParameterOverrideHelper.DeriveSecureParameterOverrides(
                resources, bicepGenerators, config.SecureParameterMappings, projectVariableGroups),
            AgentPoolName = project?.AgentPoolName,
            BicepBasePath = bicepBasePath,
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
            var req = await appPipelineRequestFactory.CreateAsync(
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

        // Upload shared app pipeline templates (only when there are app pipelines to reference them)
        if (appResult.Files.Count > 0)
        {
            foreach (var (path, content) in AppPipelineGenerationEngine.GenerateSharedTemplates())
            {
                var uri = await artifactService.UploadArtifactAsync(
                    "pipeline", command.InfrastructureConfigId, timestamp, path, content);
                fileUris[path] = uri;
            }
        }

        return new GeneratePipelineResult(fileUris);
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

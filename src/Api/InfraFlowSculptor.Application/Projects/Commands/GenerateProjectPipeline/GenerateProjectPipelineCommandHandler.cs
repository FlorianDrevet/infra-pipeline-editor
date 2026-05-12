using ErrorOr;
using InfraFlowSculptor.Application.Common.Generation;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;
using MediatR;
using AppPipelineMode = InfraFlowSculptor.GenerationCore.Models.AppPipelineMode;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectPipeline;

/// <summary>Handles the <see cref="GenerateProjectPipelineCommand"/>.</summary>
public sealed class GenerateProjectPipelineCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IInfrastructureConfigReadRepository configReadRepository,
    PipelineGenerationEngine pipelineGenerationEngine,
    AppPipelineGenerationEngine appPipelineGenerationEngine,
    IAppPipelineRequestFactory appPipelineRequestFactory,
    IEnumerable<IResourceTypeBicepSpecGenerator> bicepGenerators,
    IBlobService blobService,
    IRepositoryTargetResolver targetResolver)
    : ICommandHandler<GenerateProjectPipelineCommand, GenerateProjectPipelineResult>
{


    /// <inheritdoc />
    public async Task<ErrorOr<GenerateProjectPipelineResult>> Handle(
        GenerateProjectPipelineCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Verify write access
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var projectForGate = authResult.Value;

        // 2. Load all configurations for this project
        var configs = await configReadRepository.GetAllByProjectIdWithResourcesAsync(
            command.ProjectId.Value, cancellationToken);

        if (configs.Count == 0)
            return Errors.Project.NoConfigurationsError();

        // Reject project-level generate-all for heterogeneous multi-repo topologies.
        if (!projectForGate.CanGenerateAllFromProjectLevel())
            return Errors.GitRouting.AmbiguousProjectLevelGeneration;

        // 3. Load the enriched project snapshot needed for repository routing and variable groups.
        var project = await projectRepository.GetByIdWithAllAndPipelineVariableGroupsAsync(
            command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var projectVariableGroups = project.PipelineVariableGroups.ToList();

        // Resolve the project-level target (alias "default") to determine base paths within the repo.
        // Heterogeneous multi-repo projects will simply fall back to null paths here — the per-config
        // push handlers are responsible for enforcing the routing at push time.
        string? bicepBasePath = null;
        string? pipelineBasePath = null;
        var targetResult = targetResolver.Resolve(project, config: null, ArtifactKind.Pipeline);
        if (!targetResult.IsError)
        {
            bicepBasePath = targetResult.Value.BasePath;
            pipelineBasePath = targetResult.Value.PipelineBasePath;
        }

        // 4. Generate pipeline YAML per config (mono-repo mode: no per-config variables)
        var perConfigResults = new Dictionary<string, PipelineGenerationResult>();

        foreach (var config in configs)
        {
            var generationRequest = BuildGenerationRequest(config, projectVariableGroups, project, bicepGenerators, bicepBasePath);
            var result = pipelineGenerationEngine.Generate(generationRequest, config.Name, isMonoRepo: true);

            // Generate app pipelines for compute resources in this config
            var appResult = await GenerateAppPipelinesForConfigAsync(config, project, cancellationToken)
                .ConfigureAwait(false);

            // Merge app pipeline files into the infra result
            if (appResult.Files.Count > 0)
            {
                var mergedFiles = new Dictionary<string, string>(result.Files);
                foreach (var (path, content) in appResult.Files)
                    mergedFiles[path] = content;

                result = new PipelineGenerationResult { TemplateFiles = mergedFiles };
            }

            perConfigResults[config.Name] = result;
        }

        // 5. Collect unique environment definitions across all configs (dedup by ShortName)
        var environments = configs
            .SelectMany(c => c.Environments)
            .GroupBy(e => e.ShortName.ToLowerInvariant())
            .Select(g => g.First())
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

        // 6. Assemble mono-repo output
        var agentPoolName = project.AgentPoolName;
        var assembled = MonoRepoPipelineAssembler.Assemble(
            perConfigResults,
            environments,
            agentPoolName,
            bicepBasePath,
            pipelineBasePath);

        // 7. Upload to blob storage.
        // Layout (since SplitInfraCode dual-push):
        //   {prefix}/infra/.azuredevops/...   \u2192 infra shared templates
        //   {prefix}/app/.azuredevops/...     \u2192 app shared templates (when any app pipeline exists)
        //   {prefix}/infra/{configName}/...   \u2192 infra per-config files
        //   {prefix}/app/{configName}/...     \u2192 app per-config files (apps/ wrappers)
        var prefix = $"pipeline/project/{command.ProjectId.Value}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

        var infraCommonUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
        var appCommonUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
        foreach (var (path, content) in assembled.CommonFiles)
        {
            var repoRelativePath = $".azuredevops/Common/{path}";
            var uri = await blobService.UploadContentAsync(
                $"{prefix}/infra/{repoRelativePath}", content, "text/plain");
            infraCommonUris[repoRelativePath] = uri;
        }

        // Upload shared application pipeline templates whenever any per-config bucket emitted apps/ wrappers.
        var hasAppPipelines = assembled.ConfigFiles.Values
            .Any(files => files.Keys.Any(p => p.StartsWith("apps/", StringComparison.Ordinal)));

        if (hasAppPipelines)
        {
            foreach (var (path, content) in AppPipelineGenerationEngine.GenerateSharedTemplates())
            {
                var repoRelativePath = ToCommonAzureDevOpsPath(path);
                var uri = await blobService.UploadContentAsync(
                    $"{prefix}/app/{repoRelativePath}", content, "text/plain");
                appCommonUris[repoRelativePath] = uri;
            }
        }

        var infraConfigUris = new Dictionary<string, IReadOnlyDictionary<string, Uri>>(StringComparer.Ordinal);
        var appConfigUris = new Dictionary<string, IReadOnlyDictionary<string, Uri>>(StringComparer.Ordinal);
        var unionConfigUris = new Dictionary<string, IReadOnlyDictionary<string, Uri>>(StringComparer.Ordinal);

        foreach (var (configName, files) in assembled.ConfigFiles)
        {
            var (infraFiles, appFiles) = AppPipelineFileClassifier.Split(files);

            var infraUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
            foreach (var (path, content) in infraFiles)
            {
                var repoRelativePath = $".azuredevops/{configName}/{path}";
                var uri = await blobService.UploadContentAsync(
                    $"{prefix}/infra/{repoRelativePath}", content, "text/plain");
                infraUris[repoRelativePath] = uri;
            }

            var appUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
            foreach (var (path, content) in appFiles)
            {
                var repoRelativePath = $".azuredevops/{configName}/{path}";
                var uri = await blobService.UploadContentAsync(
                    $"{prefix}/app/{repoRelativePath}", content, "text/plain");
                appUris[repoRelativePath] = uri;
            }

            infraConfigUris[configName] = infraUris;
            appConfigUris[configName] = appUris;

            // Union view (legacy CommonFileUris/ConfigFileUris consumers expect a flat per-config map).
            var union = new Dictionary<string, Uri>(infraUris, StringComparer.Ordinal);
            foreach (var (path, uri) in appUris)
                union[path] = uri;
            unionConfigUris[configName] = union;
        }

        var unionCommonUris = new Dictionary<string, Uri>(infraCommonUris, StringComparer.Ordinal);
        foreach (var (path, uri) in appCommonUris)
            unionCommonUris[path] = uri;

        return new GenerateProjectPipelineResult(
            CommonFileUris: unionCommonUris,
            ConfigFileUris: unionConfigUris,
            InfraCommonFileUris: infraCommonUris,
            AppCommonFileUris: appCommonUris,
            InfraConfigFileUris: infraConfigUris,
            AppConfigFileUris: appConfigUris);
    }

    private static string ToCommonAzureDevOpsPath(string path)
    {
        const string azureDevOpsPrefix = ".azuredevops/";

        return path.StartsWith(azureDevOpsPrefix, StringComparison.Ordinal)
            ? $".azuredevops/Common/{path[azureDevOpsPrefix.Length..]}"
            : $".azuredevops/Common/{path}";
    }

    private static GenerationRequest BuildGenerationRequest(
        InfrastructureConfigReadModel config,
        List<Domain.ProjectAggregate.Entities.ProjectPipelineVariableGroup> projectVariableGroups,
        Domain.ProjectAggregate.Project? project,
        IEnumerable<IResourceTypeBicepSpecGenerator> generators,
        string? bicepBasePath)
    {
        var mergedAbbreviations = MergeAbbreviations(config.NamingContext.ResourceAbbreviations);

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

        // Derive PVG mappings from app settings linked to each variable group
        var pipelineVariableGroups = projectVariableGroups
            .Select(g =>
            {
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
            })
            .ToList();

        return new GenerationRequest
        {
            Resources = resources,
            ResourceGroups = resourceGroups,
            Environments = environments,
            EnvironmentNames = environmentNames,
            NamingContext = namingContext,
            RoleAssignments = [],
            AppSettings = [],
            ExistingResourceReferences = [],
            PipelineVariableGroups = pipelineVariableGroups,
            SecureParameterOverrides = SecureParameterOverrideHelper.DeriveSecureParameterOverrides(
                resources, generators, config.SecureParameterMappings, pipelineVariableGroups),
            AgentPoolName = project?.AgentPoolName,
            BicepBasePath = bicepBasePath,
        };
    }

    private static string GetResourceAbbreviation(
        string azureResourceType,
        IReadOnlyDictionary<string, string> mergedAbbreviations)
    {
        var typeName = AzureResourceTypes.GetFriendlyName(azureResourceType);
        return mergedAbbreviations.TryGetValue(typeName, out var abbr)
            ? abbr
            : ResourceAbbreviationCatalog.GetAbbreviation(typeName);
    }

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

    /// <summary>
    /// Generates app pipelines for all compute resources in the given configuration.
    /// </summary>
    private async Task<AppPipelineGenerationResult> GenerateAppPipelinesForConfigAsync(
        InfrastructureConfigReadModel config,
        Domain.ProjectAggregate.Project? project,
        CancellationToken cancellationToken)
    {
        var computeTypes = new HashSet<string>
        {
            AzureResourceTypes.ArmTypes.ContainerAppType,
            AzureResourceTypes.ArmTypes.WebAppType,
            AzureResourceTypes.ArmTypes.FunctionAppType,
        };

        var computeResources = config.ResourceGroups
            .SelectMany(rg => rg.Resources)
            .Where(r => computeTypes.Contains(r.ResourceType))
            .ToList();

        var appRequests = new List<AppPipelineGenerationRequest>();

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

        foreach (var resource in computeResources)
        {
            var resourceId = new AzureResourceId(resource.Id);
            var req = await appPipelineRequestFactory.CreateAsync(
                resourceId, resource.ResourceType, cancellationToken).ConfigureAwait(false);

            if (req is null)
                continue;

            req.ConfigName = config.Name;
            req.Environments = environments;
            req.IsMonoRepo = true;
            req.AgentPoolName = project?.AgentPoolName;

            appRequests.Add(req);
        }

        var appPipelineMode = Enum.TryParse<AppPipelineMode>(config.AppPipelineMode, out var parsedMode)
            ? parsedMode
            : AppPipelineMode.Isolated;

        return appPipelineGenerationEngine.GenerateAll(appRequests, appPipelineMode, config.Name);
    }

}

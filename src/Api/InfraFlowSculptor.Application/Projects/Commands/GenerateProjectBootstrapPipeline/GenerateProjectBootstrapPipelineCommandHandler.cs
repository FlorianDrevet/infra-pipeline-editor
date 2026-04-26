using System.Net;
using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;

/// <summary>Handles the <see cref="GenerateProjectBootstrapPipelineCommand"/>.</summary>
public sealed class GenerateProjectBootstrapPipelineCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IInfrastructureConfigReadRepository configReadRepository,
    IContainerAppRepository containerAppRepository,
    IWebAppRepository webAppRepository,
    IFunctionAppRepository functionAppRepository,
    BootstrapPipelineGenerationEngine bootstrapEngine,
    IBlobService blobService,
    IRepositoryTargetResolver targetResolver)
    : ICommandHandler<GenerateProjectBootstrapPipelineCommand, GenerateProjectBootstrapPipelineResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<GenerateProjectBootstrapPipelineResult>> Handle(
        GenerateProjectBootstrapPipelineCommand command,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        // Resolve the project-level target (alias "default") for bootstrap artifacts.
        var targetResult = targetResolver.Resolve(project, config: null, ArtifactKind.Bootstrap);
        if (targetResult.IsError)
            return targetResult.Errors;

        var target = targetResult.Value;

        var configs = await configReadRepository.GetAllByProjectIdWithResourcesAsync(
            command.ProjectId.Value,
            cancellationToken);

        if (configs.Count == 0)
            return Errors.Project.NoConfigurationsError();

        var projectWithVariableGroups = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
            command.ProjectId,
            cancellationToken);

        if (projectWithVariableGroups is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var ownerParts = target.Owner.Split('/', 2);
        var organizationName = DecodeUrlSegment(ownerParts[0]);
        var adoProjectName = DecodeUrlSegment(ownerParts.Length > 1 ? ownerParts[1] : ownerParts[0]);

        var isSplit = project.LayoutPreset.Value == LayoutPresetEnum.SplitInfraCode;

        ResolvedRepositoryTarget? appTarget = null;
        if (isSplit)
        {
            var appTargetResult = targetResolver.Resolve(project, config: null, ArtifactKind.BootstrapApplication);
            if (appTargetResult.IsError)
                return appTargetResult.Errors;
            appTarget = appTargetResult.Value;
        }

        var (infraPipelines, appPipelines) = await BuildPipelineDefinitionsSplitAsync(
                configs,
                infraPipelineBasePath: target.PipelineBasePath,
                appPipelineBasePath: isSplit ? appTarget!.PipelineBasePath : target.PipelineBasePath,
                cancellationToken)
            .ConfigureAwait(false);

        var projectVariableGroups = projectWithVariableGroups.PipelineVariableGroups.ToList();
        var variableGroupUsages = await projectRepository.GetPipelineVariableUsagesAsync(
            projectVariableGroups.Select(group => group.Id).ToList(),
            cancellationToken);

        var environments = configs
            .SelectMany(config => config.Environments)
            .GroupBy(environment => environment.ShortName.ToLowerInvariant())
            .Select(group => group.First())
            .ToList();

        var variableGroups = BuildVariableGroupDefinitions(
            projectVariableGroups,
            variableGroupUsages,
            configs,
            environments);

        var bootstrapEnvironments = BuildEnvironmentDefinitions(project.EnvironmentDefinitions, configs);

        var prefix = $"bootstrap/project/{command.ProjectId.Value}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var unionFileUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
        var infraFileUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
        var appFileUris = new Dictionary<string, Uri>(StringComparer.Ordinal);

        if (isSplit)
        {
            // Infra bootstrap (FullOwner): infra pipelines + envs + VGs.
            var infraRequest = new BootstrapGenerationRequest
            {
                OrganizationName = organizationName,
                ProjectName = adoProjectName,
                RepositoryName = DecodeUrlSegment(target.RepositoryName),
                DefaultBranch = target.Branch,
                AgentPoolName = project.AgentPoolName,
                Pipelines = infraPipelines,
                Environments = bootstrapEnvironments,
                VariableGroups = variableGroups,
                Mode = BootstrapMode.FullOwner,
            };

            var infraGeneration = bootstrapEngine.Generate(infraRequest);
            foreach (var (path, content) in infraGeneration.Files)
            {
                var uri = await blobService.UploadContentAsync($"{prefix}/infra/{path}", content, "text/plain");
                infraFileUris[path] = uri;
                unionFileUris[$"infra/{path}"] = uri;
            }

            // Application bootstrap (ApplicationOnly): app pipelines + validation of envs/VGs.
            var appRequest = new BootstrapGenerationRequest
            {
                OrganizationName = organizationName,
                ProjectName = adoProjectName,
                RepositoryName = DecodeUrlSegment(appTarget!.RepositoryName),
                DefaultBranch = appTarget.Branch,
                AgentPoolName = project.AgentPoolName,
                Pipelines = appPipelines,
                Environments = bootstrapEnvironments,
                VariableGroups = variableGroups,
                Mode = BootstrapMode.ApplicationOnly,
            };

            var appGeneration = bootstrapEngine.Generate(appRequest);
            foreach (var (path, content) in appGeneration.Files)
            {
                var uri = await blobService.UploadContentAsync($"{prefix}/app/{path}", content, "text/plain");
                appFileUris[path] = uri;
                unionFileUris[$"app/{path}"] = uri;
            }
        }
        else
        {
            // AllInOne / MultiRepo: single bootstrap owns everything (infra + app pipelines).
            var allPipelines = infraPipelines.Concat(appPipelines)
                .DistinctBy(pipeline => new { pipeline.Name, pipeline.YamlPath, pipeline.Folder })
                .ToList();

            var bootstrapRequest = new BootstrapGenerationRequest
            {
                OrganizationName = organizationName,
                ProjectName = adoProjectName,
                RepositoryName = DecodeUrlSegment(target.RepositoryName),
                DefaultBranch = target.Branch,
                AgentPoolName = project.AgentPoolName,
                Pipelines = allPipelines,
                Environments = bootstrapEnvironments,
                VariableGroups = variableGroups,
                Mode = BootstrapMode.FullOwner,
            };

            var generationResult = bootstrapEngine.Generate(bootstrapRequest);

            foreach (var (path, content) in generationResult.Files)
            {
                var uri = await blobService.UploadContentAsync($"{prefix}/{path}", content, "text/plain");
                unionFileUris[path] = uri;
                infraFileUris[path] = uri;
            }
        }

        return new GenerateProjectBootstrapPipelineResult(unionFileUris, infraFileUris, appFileUris);
    }

    private async Task<(IReadOnlyList<BootstrapPipelineDefinition> Infra, IReadOnlyList<BootstrapPipelineDefinition> App)>
        BuildPipelineDefinitionsSplitAsync(
        IReadOnlyList<InfrastructureConfigReadModel> configs,
        string? infraPipelineBasePath,
        string? appPipelineBasePath,
        CancellationToken cancellationToken)
    {
        var infraPipelines = new List<BootstrapPipelineDefinition>();
        var appPipelines = new List<BootstrapPipelineDefinition>();

        var infraBasePrefix = string.IsNullOrEmpty(infraPipelineBasePath)
            ? string.Empty
            : $"{infraPipelineBasePath.Trim('/')}/";

        var appBasePrefix = string.IsNullOrEmpty(appPipelineBasePath)
            ? string.Empty
            : $"{appPipelineBasePath.Trim('/')}/";

        foreach (var config in configs)
        {
            var sanitizedConfigName = PathSanitizer.Sanitize(config.Name);

            infraPipelines.AddRange(BuildInfrastructurePipelineDefinitions(config.Name, sanitizedConfigName, infraBasePrefix));
            appPipelines.AddRange(await BuildApplicationPipelineDefinitionsAsync(
                    config,
                    sanitizedConfigName,
                    appBasePrefix,
                    cancellationToken)
                .ConfigureAwait(false));
        }

        return (
            infraPipelines.DistinctBy(pipeline => new { pipeline.Name, pipeline.YamlPath, pipeline.Folder }).ToList(),
            appPipelines.DistinctBy(pipeline => new { pipeline.Name, pipeline.YamlPath, pipeline.Folder }).ToList());
    }

    private static IReadOnlyList<BootstrapPipelineDefinition> BuildInfrastructurePipelineDefinitions(
        string configName,
        string sanitizedConfigName,
        string basePrefix)
    {
        return
        [
            new BootstrapPipelineDefinition(
                Name: $"{sanitizedConfigName} - CI",
                YamlPath: $"/{basePrefix}.azuredevops/{sanitizedConfigName}/ci.pipeline.yml",
                Folder: $"\\{sanitizedConfigName}"),
            new BootstrapPipelineDefinition(
                Name: $"{sanitizedConfigName} - PR",
                YamlPath: $"/{basePrefix}.azuredevops/{sanitizedConfigName}/pr.pipeline.yml",
                Folder: $"\\{sanitizedConfigName}"),
            new BootstrapPipelineDefinition(
                Name: $"{sanitizedConfigName} - Release",
                YamlPath: $"/{basePrefix}.azuredevops/{sanitizedConfigName}/release.pipeline.yml",
                Folder: $"\\{sanitizedConfigName}"),
        ];
    }

    private async Task<IReadOnlyList<BootstrapPipelineDefinition>> BuildApplicationPipelineDefinitionsAsync(
        InfrastructureConfigReadModel config,
        string sanitizedConfigName,
        string basePrefix,
        CancellationToken cancellationToken)
    {
        var pipelines = new List<BootstrapPipelineDefinition>();

        var computeResources = config.ResourceGroups
            .SelectMany(resourceGroup => resourceGroup.Resources)
            .Where(resource => !resource.IsExisting && IsApplicationPipelineResource(resource.ResourceType))
            .ToList();

        foreach (var resource in computeResources)
        {
            var appFolderName = await ResolveApplicationFolderNameAsync(resource, cancellationToken)
                .ConfigureAwait(false);
            var sanitizedAppName = PathSanitizer.Sanitize(appFolderName);
            var yamlBasePath = $"/{basePrefix}.azuredevops/{sanitizedConfigName}/apps/{sanitizedAppName}";
            var folder = $"\\{sanitizedConfigName}\\Applications\\{sanitizedAppName}";

            pipelines.Add(new BootstrapPipelineDefinition(
                Name: $"{config.Name} - {resource.Name} - CI",
                YamlPath: $"{yamlBasePath}/ci.app-pipeline.yml",
                Folder: folder));

            pipelines.Add(new BootstrapPipelineDefinition(
                Name: $"{config.Name} - {resource.Name} - Release",
                YamlPath: $"{yamlBasePath}/release.app-pipeline.yml",
                Folder: folder));
        }

        return pipelines;
    }

    private static bool IsApplicationPipelineResource(string resourceType)
    {
        return resourceType is AzureResourceTypes.ArmTypes.ContainerApp
            or AzureResourceTypes.ArmTypes.WebApp
            or AzureResourceTypes.ArmTypes.FunctionApp;
    }

    private async Task<string> ResolveApplicationFolderNameAsync(
        AzureResourceReadModel resource,
        CancellationToken cancellationToken)
    {
        var resourceId = new AzureResourceId(resource.Id);

        return resource.ResourceType switch
        {
            AzureResourceTypes.ArmTypes.ContainerApp =>
                (await containerAppRepository.GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false))?.ApplicationName
                ?? resource.Name,
            AzureResourceTypes.ArmTypes.WebApp =>
                (await webAppRepository.GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false))?.ApplicationName
                ?? resource.Name,
            AzureResourceTypes.ArmTypes.FunctionApp =>
                (await functionAppRepository.GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false))?.ApplicationName
                ?? resource.Name,
            _ => resource.Name,
        };
    }

    private static IReadOnlyList<BootstrapVariableGroupDefinition> BuildVariableGroupDefinitions(
        List<ProjectPipelineVariableGroup> projectVariableGroups,
        IReadOnlyDictionary<Guid, List<PipelineVariableUsageResult>> variableGroupUsages,
        IReadOnlyList<InfrastructureConfigReadModel> configs,
        IReadOnlyList<EnvironmentDefinitionReadModel> environments)
    {
        var result = new List<BootstrapVariableGroupDefinition>();

        foreach (var group in projectVariableGroups)
        {
            var groupNames = ExpandGroupName(group.GroupName, environments);
            var secretVariableNames = configs
                .SelectMany(config =>
                    (config.SecureParameterMappings ?? [])
                        .Where(mapping => mapping.VariableGroupId == group.Id.Value && mapping.PipelineVariableName is not null)
                        .Select(mapping => mapping.PipelineVariableName!)
                    .Concat(config.AppSettings
                        .Where(setting =>
                            setting.VariableGroupId == group.Id.Value
                            && setting.PipelineVariableName is not null
                            && setting.IsKeyVaultReference
                            && string.Equals(setting.SecretValueAssignment, "ViaBicepparam", StringComparison.OrdinalIgnoreCase))
                        .Select(setting => setting.PipelineVariableName!)))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var variableNames = variableGroupUsages.TryGetValue(group.Id.Value, out var usages)
                ? usages.Select(usage => usage.PipelineVariableName)
                : Enumerable.Empty<string>();

            var variables = variableNames
                .Concat(secretVariableNames)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .Select(name => new BootstrapVariable(name, string.Empty, secretVariableNames.Contains(name)))
                .ToList();

            foreach (var expandedGroupName in groupNames)
            {
                if (variables.Count > 0)
                    result.Add(new BootstrapVariableGroupDefinition(expandedGroupName, variables));
                else
                    result.Add(new BootstrapVariableGroupDefinition(expandedGroupName, []));
            }
        }

        return result;
    }

    private static IReadOnlyList<BootstrapEnvironmentDefinition> BuildEnvironmentDefinitions(
        IReadOnlyCollection<ProjectEnvironmentDefinition> projectEnvironments,
        IReadOnlyList<InfrastructureConfigReadModel> configs)
    {
        if (projectEnvironments.Count > 0)
        {
            return projectEnvironments
                .OrderBy(environment => environment.Order.Value)
                .GroupBy(environment => environment.ShortName.Value, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .Select(environment => new BootstrapEnvironmentDefinition(
                    Name: environment.ShortName.Value.ToLowerInvariant(),
                    DisplayName: environment.Name.Value,
                    RequiresApproval: environment.RequiresApproval.Value))
                .ToList();
        }

        return configs
            .SelectMany(config => config.Environments)
            .GroupBy(environment => environment.ShortName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(environment => environment.ShortName, StringComparer.OrdinalIgnoreCase)
            .Select(environment => new BootstrapEnvironmentDefinition(
                Name: environment.ShortName.ToLowerInvariant(),
                DisplayName: environment.Name,
                RequiresApproval: false))
            .ToList();
    }

    /// <summary>
    /// Expands a group name that may contain <c>{env}</c> placeholders into one name per environment.
    /// Group names without a placeholder are returned as-is.
    /// </summary>
    private static IEnumerable<string> ExpandGroupName(
        string groupName,
        IReadOnlyList<EnvironmentDefinitionReadModel> environments)
    {
        if (!groupName.Contains("{env}", StringComparison.OrdinalIgnoreCase))
            return [groupName];

        return environments
            .Select(environment => groupName.Replace("{env}", environment.ShortName, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static string DecodeUrlSegment(string value) =>
        WebUtility.UrlDecode(value);
}

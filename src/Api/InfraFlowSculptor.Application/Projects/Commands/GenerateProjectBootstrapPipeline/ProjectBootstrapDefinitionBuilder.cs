using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;

/// <summary>
/// Builds the bootstrap definitions previously assembled inline by the command handler.
/// </summary>
public sealed class ProjectBootstrapDefinitionBuilder(
    IProjectRepository projectRepository,
    IApplicationFolderNameResolver applicationFolderNameResolver,
    IContainerAppRepository containerAppRepository)
    : IProjectBootstrapDefinitionBuilder
{
    /// <inheritdoc />
    public async Task<ProjectBootstrapDefinitions> BuildAsync(
        Project project,
        IReadOnlyList<InfrastructureConfigReadModel> configs,
        string? infraPipelineBasePath,
        string? appPipelineBasePath,
        CancellationToken cancellationToken)
    {
        var (infraPipelines, appPipelines) = await BuildPipelineDefinitionsAsync(
                configs,
                infraPipelineBasePath,
                appPipelineBasePath,
                cancellationToken)
            .ConfigureAwait(false);

        var projectVariableGroups = project.PipelineVariableGroups.ToList();
        var variableGroupUsages = await projectRepository.GetPipelineVariableUsagesAsync(
                projectVariableGroups.Select(group => group.Id).ToList(),
                cancellationToken)
            .ConfigureAwait(false);

        var environments = configs
            .SelectMany(config => config.Environments)
            .GroupBy(environment => environment.ShortName.ToLowerInvariant())
            .SelectMany(group => group.Take(1))
            .ToList();

        var variableGroups = BuildVariableGroupDefinitions(
            projectVariableGroups,
            variableGroupUsages,
            configs,
            environments);

        var bootstrapEnvironments = BuildEnvironmentDefinitions(project.EnvironmentDefinitions, configs);

        var serviceConnections = await BuildServiceConnectionDefinitionsAsync(project, configs, cancellationToken)
            .ConfigureAwait(false);

        return new ProjectBootstrapDefinitions(
            infraPipelines,
            appPipelines,
            variableGroups,
            bootstrapEnvironments,
            serviceConnections);
    }

    private async Task<(IReadOnlyList<BootstrapPipelineDefinition> Infra, IReadOnlyList<BootstrapPipelineDefinition> App)>
        BuildPipelineDefinitionsAsync(
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

            infraPipelines.AddRange(BuildInfrastructurePipelineDefinitions(sanitizedConfigName, infraBasePrefix));
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
        string sanitizedConfigName,
        string basePrefix)
    {
        return
        [
            new BootstrapPipelineDefinition(
                Name: AzureDevOpsPipelineNameHelper.BuildInfrastructureCiName(sanitizedConfigName),
                YamlPath: $"/{basePrefix}.azuredevops/{sanitizedConfigName}/ci.pipeline.yml",
                Folder: $"\\{sanitizedConfigName}"),
            new BootstrapPipelineDefinition(
                Name: AzureDevOpsPipelineNameHelper.BuildInfrastructurePrName(sanitizedConfigName),
                YamlPath: $"/{basePrefix}.azuredevops/{sanitizedConfigName}/pr.pipeline.yml",
                Folder: $"\\{sanitizedConfigName}"),
            new BootstrapPipelineDefinition(
                Name: AzureDevOpsPipelineNameHelper.BuildInfrastructureReleaseName(sanitizedConfigName),
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
            var appFolderName = await applicationFolderNameResolver.ResolveAsync(resource, cancellationToken)
                .ConfigureAwait(false);
            var sanitizedAppName = PathSanitizer.Sanitize(appFolderName);
            var yamlBasePath = $"/{basePrefix}.azuredevops/{sanitizedConfigName}/apps/{sanitizedAppName}";
            var folder = $"\\{sanitizedConfigName}\\Applications\\{sanitizedAppName}";

            pipelines.Add(new BootstrapPipelineDefinition(
                Name: AzureDevOpsPipelineNameHelper.BuildApplicationCiName(config.Name, resource.Name),
                YamlPath: $"{yamlBasePath}/ci.app-pipeline.yml",
                Folder: folder));

            pipelines.Add(new BootstrapPipelineDefinition(
                Name: AzureDevOpsPipelineNameHelper.BuildApplicationReleaseName(config.Name, resource.Name),
                YamlPath: $"{yamlBasePath}/release.app-pipeline.yml",
                Folder: folder));
        }

        return pipelines;
    }

    private static bool IsApplicationPipelineResource(string resourceType)
    {
        return resourceType is AzureResourceTypes.ArmTypes.ContainerAppType
            or AzureResourceTypes.ArmTypes.WebAppType
            or AzureResourceTypes.ArmTypes.FunctionAppType;
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
                            && string.Equals(
                                setting.SecretValueAssignment,
                                nameof(SecretValueAssignment.ViaBicepparam),
                                StringComparison.OrdinalIgnoreCase))
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
                result.Add(variables.Count > 0
                    ? new BootstrapVariableGroupDefinition(expandedGroupName, variables)
                    : new BootstrapVariableGroupDefinition(expandedGroupName, []));
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
                .SelectMany(group => group.Take(1))
                .Select(environment => new BootstrapEnvironmentDefinition(
                    Name: environment.ShortName.Value.ToLowerInvariant(),
                    DisplayName: environment.Name.Value,
                    RequiresApproval: environment.RequiresApproval.Value))
                .ToList();
        }

        return configs
            .SelectMany(config => config.Environments)
            .GroupBy(environment => environment.ShortName, StringComparer.OrdinalIgnoreCase)
            .SelectMany(group => group.Take(1))
            .OrderBy(environment => environment.ShortName, StringComparer.OrdinalIgnoreCase)
            .Select(environment => new BootstrapEnvironmentDefinition(
                Name: environment.ShortName.ToLowerInvariant(),
                DisplayName: environment.Name,
                RequiresApproval: false))
            .ToList();
    }

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

    private async Task<IReadOnlyList<BootstrapServiceConnectionDefinition>> BuildServiceConnectionDefinitionsAsync(
        Project project,
        IReadOnlyList<InfrastructureConfigReadModel> configs,
        CancellationToken cancellationToken)
    {
        var definitions = new List<BootstrapServiceConnectionDefinition>();

        // ARM service connections from project environment definitions.
        foreach (var env in project.EnvironmentDefinitions)
        {
            if (!string.IsNullOrWhiteSpace(env.AzureResourceManagerConnection))
            {
                definitions.Add(new BootstrapServiceConnectionDefinition(
                    env.AzureResourceManagerConnection,
                    BootstrapServiceConnectionTypes.AzureRM,
                    env.ShortName.Value));
            }
        }

        // Container Registry service connections from Container App environment settings.
        var containerAppResourceIds = configs
            .SelectMany(config => config.ResourceGroups)
            .SelectMany(rg => rg.Resources)
            .Where(resource => !resource.IsExisting
                               && resource.ResourceType == AzureResourceTypes.ArmTypes.ContainerAppType)
            .Select(resource => new AzureResourceId(resource.Id))
            .ToList();

        foreach (var resourceId in containerAppResourceIds)
        {
            var containerApp = await containerAppRepository.GetByIdReadOnlyAsync(resourceId, cancellationToken)
                .ConfigureAwait(false);

            if (containerApp is null)
                continue;

            foreach (var envSettings in containerApp.EnvironmentSettings)
            {
                if (!string.IsNullOrWhiteSpace(envSettings.ContainerRegistryServiceConnection))
                {
                    definitions.Add(new BootstrapServiceConnectionDefinition(
                        envSettings.ContainerRegistryServiceConnection,
                        BootstrapServiceConnectionTypes.DockerRegistry,
                        envSettings.EnvironmentName));
                }
            }
        }

        return definitions
            .DistinctBy(sc => sc.Name, StringComparer.OrdinalIgnoreCase)
            .OrderBy(sc => sc.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

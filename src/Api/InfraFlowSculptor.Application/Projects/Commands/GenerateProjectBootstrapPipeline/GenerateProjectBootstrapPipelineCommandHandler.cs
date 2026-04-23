using System.Net;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;

/// <summary>Handles the <see cref="GenerateProjectBootstrapPipelineCommand"/>.</summary>
public sealed class GenerateProjectBootstrapPipelineCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IInfrastructureConfigReadRepository configReadRepository,
    BootstrapPipelineGenerationEngine bootstrapEngine,
    IBlobService blobService)
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

        if (project.GitRepositoryConfiguration is null)
            return Errors.GitRepository.NotConfigured();

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

        var gitConfig = project.GitRepositoryConfiguration;
        var ownerParts = gitConfig.Owner.Split('/', 2);
        var organizationName = DecodeUrlSegment(ownerParts[0]);
        var adoProjectName = DecodeUrlSegment(ownerParts.Length > 1 ? ownerParts[1] : ownerParts[0]);

        var pipelineBasePath = gitConfig.PipelineBasePath;
        var pipelines = BuildPipelineDefinitions(configs, pipelineBasePath);

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

        var bootstrapRequest = new BootstrapGenerationRequest
        {
            OrganizationName = organizationName,
            ProjectName = adoProjectName,
            RepositoryName = DecodeUrlSegment(gitConfig.RepositoryName),
            DefaultBranch = gitConfig.DefaultBranch,
            AgentPoolName = project.AgentPoolName,
            Pipelines = pipelines,
            Environments = bootstrapEnvironments,
            VariableGroups = variableGroups,
        };

        var generationResult = bootstrapEngine.Generate(bootstrapRequest);
        var prefix = $"bootstrap/project/{command.ProjectId.Value}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var fileUris = new Dictionary<string, Uri>();

        foreach (var (path, content) in generationResult.Files)
        {
            var uri = await blobService.UploadContentAsync($"{prefix}/{path}", content, "text/plain");
            fileUris[path] = uri;
        }

        return new GenerateProjectBootstrapPipelineResult(fileUris);
    }

    private static IReadOnlyList<BootstrapPipelineDefinition> BuildPipelineDefinitions(
        IReadOnlyList<InfrastructureConfigReadModel> configs,
        string? pipelineBasePath)
    {
        var pipelines = new List<BootstrapPipelineDefinition>();
        var basePrefix = string.IsNullOrEmpty(pipelineBasePath)
            ? string.Empty
            : $"{pipelineBasePath.Trim('/')}/";

        foreach (var config in configs)
        {
            var sanitizedName = PathSanitizer.Sanitize(config.Name);

            pipelines.Add(new BootstrapPipelineDefinition(
                Name: $"{sanitizedName} - CI",
                YamlPath: $"/{basePrefix}{sanitizedName}/ci.pipeline.yml",
                Folder: $"\\{sanitizedName}"));

            pipelines.Add(new BootstrapPipelineDefinition(
                Name: $"{sanitizedName} - PR",
                YamlPath: $"/{basePrefix}{sanitizedName}/pr.pipeline.yml",
                Folder: $"\\{sanitizedName}"));

            pipelines.Add(new BootstrapPipelineDefinition(
                Name: $"{sanitizedName} - Release",
                YamlPath: $"/{basePrefix}{sanitizedName}/release.pipeline.yml",
                Folder: $"\\{sanitizedName}"));
        }

        return pipelines;
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
                .SelectMany(config => config.SecureParameterMappings ?? [])
                .Where(mapping => mapping.VariableGroupId == group.Id.Value && mapping.PipelineVariableName is not null)
                .Select(mapping => mapping.PipelineVariableName!)
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

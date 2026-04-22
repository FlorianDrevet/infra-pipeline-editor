using System.Net;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;

/// <summary>Handles the <see cref="GenerateProjectBootstrapPipelineCommand"/>.</summary>
public sealed class GenerateProjectBootstrapPipelineCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IInfrastructureConfigReadRepository configReadRepository,
    IEnumerable<IResourceTypeBicepGenerator> bicepGenerators,
    BootstrapPipelineGenerationEngine bootstrapEngine,
    IBlobService blobService)
    : ICommandHandler<GenerateProjectBootstrapPipelineCommand, GenerateProjectBootstrapPipelineResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<GenerateProjectBootstrapPipelineResult>> Handle(
        GenerateProjectBootstrapPipelineCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Verify write access
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        // 2. Load project with all data (git config, variable groups, environments)
        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        // 3. Check Git config is present (required to resolve org/project/repo names)
        if (project.GitRepositoryConfiguration is null)
            return Errors.GitRepository.NotConfigured();

        // 4. Load all infrastructure configurations for this project
        var configs = await configReadRepository.GetAllByProjectIdWithResourcesAsync(
            command.ProjectId.Value, cancellationToken);

        if (configs.Count == 0)
            return Errors.Project.NoConfigurationsError();

        // 5. Parse Azure DevOps organization and project name from Owner (format: "org/project")
        var gitConfig = project.GitRepositoryConfiguration;
        var ownerParts = gitConfig.Owner.Split('/', 2);
        var organizationName = DecodeUrlSegment(ownerParts[0]);
        var adoProjectName = DecodeUrlSegment(ownerParts.Length > 1 ? ownerParts[1] : ownerParts[0]);

        // 6. Build the list of pipeline definitions (one CI + one Release per infra config)
        var pipelineBasePath = gitConfig.PipelineBasePath;
        var pipelines = BuildPipelineDefinitions(configs, pipelineBasePath);

        // 7. Build the list of variable groups from project-level groups
        var projectVariableGroups = project.PipelineVariableGroups.ToList();
        var environments = configs
            .SelectMany(c => c.Environments)
            .GroupBy(e => e.ShortName.ToLowerInvariant())
            .Select(g => g.First())
            .ToList();

        var variableGroups = BuildVariableGroupDefinitions(
            projectVariableGroups,
            configs,
            environments,
            bicepGenerators);

        // 8. Build the bootstrap generation request
        var bootstrapRequest = new BootstrapGenerationRequest
        {
            OrganizationName = organizationName,
            ProjectName = adoProjectName,
            RepositoryName = DecodeUrlSegment(gitConfig.RepositoryName),
            DefaultBranch = gitConfig.DefaultBranch,
            AgentPoolName = project.AgentPoolName,
            Pipelines = pipelines,
            VariableGroups = variableGroups,
        };

        // 9. Generate the bootstrap YAML
        var generationResult = bootstrapEngine.Generate(bootstrapRequest);

        // 10. Upload to blob storage under bootstrap/ prefix
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
                Name: $"{config.Name} — CI",
                YamlPath: $"/{basePrefix}{sanitizedName}/ci.pipeline.yml",
                Folder: $"\\{sanitizedName}"));

            pipelines.Add(new BootstrapPipelineDefinition(
                Name: $"{config.Name} — PR",
                YamlPath: $"/{basePrefix}{sanitizedName}/pr.pipeline.yml",
                Folder: $"\\{sanitizedName}"));

            pipelines.Add(new BootstrapPipelineDefinition(
                Name: $"{config.Name} — Release",
                YamlPath: $"/{basePrefix}{sanitizedName}/release.pipeline.yml",
                Folder: $"\\{sanitizedName}"));
        }

        return pipelines;
    }

    private static IReadOnlyList<BootstrapVariableGroupDefinition> BuildVariableGroupDefinitions(
        List<Domain.ProjectAggregate.Entities.ProjectPipelineVariableGroup> projectVariableGroups,
        IReadOnlyList<InfrastructureConfigReadModel> configs,
        IReadOnlyList<EnvironmentDefinitionReadModel> environments,
        IEnumerable<IResourceTypeBicepGenerator> bicepGenerators)
    {
        var result = new List<BootstrapVariableGroupDefinition>();

        foreach (var group in projectVariableGroups)
        {
            var groupNames = ExpandGroupName(group.GroupName, environments);

            foreach (var expandedGroupName in groupNames)
            {
                var variables = new List<BootstrapVariable>();

                // Collect pipeline variable names mapped to this group (from app settings across all configs)
                var mappedPipelineVarNames = configs
                    .SelectMany(c => c.AppSettings)
                    .Where(s => s.IsViaVariableGroup && s.VariableGroupId.HasValue
                        && s.VariableGroupId.Value == group.Id.Value
                        && s.PipelineVariableName is not null)
                    .Select(s => s.PipelineVariableName!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var varName in mappedPipelineVarNames)
                {
                    variables.Add(new BootstrapVariable(varName, string.Empty, IsSecret: false));
                }

                // Collect secure parameter overrides mapped to this group
                var secureParamNames = configs
                    .SelectMany(c => c.SecureParameterMappings ?? [])
                    .Where(m => m.VariableGroupId == group.Id.Value && m.PipelineVariableName is not null)
                    .Select(m => m.PipelineVariableName!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var secretName in secureParamNames)
                {
                    if (!variables.Any(v => string.Equals(v.Name, secretName, StringComparison.OrdinalIgnoreCase)))
                        variables.Add(new BootstrapVariable(secretName, string.Empty, IsSecret: true));
                }

                if (variables.Count > 0)
                    result.Add(new BootstrapVariableGroupDefinition(expandedGroupName, variables));
                else
                    result.Add(new BootstrapVariableGroupDefinition(expandedGroupName, []));
            }
        }

        return result;
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
            .Select(e => groupName.Replace("{env}", e.ShortName, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static string DecodeUrlSegment(string value) =>
        WebUtility.UrlDecode(value);
}

using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.Common.Errors;
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

        // Load project-level pipeline variable groups
        var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
            new ProjectId(config.ProjectId), cancellationToken);

        var projectVariableGroups = project?.PipelineVariableGroups
            .Select(g => new PipelineVariableGroupDefinition
            {
                GroupName = g.GroupName,
                Mappings = g.Mappings.Select(m => new PipelineVariableMappingDefinition
                {
                    PipelineVariableName = m.PipelineVariableName,
                    BicepParameterName = m.BicepParameterName,
                }).ToList(),
            }).ToList() ?? [];

        var resources = config.ResourceGroups
            .SelectMany(rg => rg.Resources.Select(r => new ResourceDefinition
            {
                Name = r.Name,
                Type = r.ResourceType,
                ResourceGroupName = rg.Name,
                Sku = r.Properties.GetValueOrDefault("sku", string.Empty),
                Properties = r.Properties,
                ResourceAbbreviation = ResourceAbbreviationCatalog.GetAbbreviation(
                    GetResourceTypeName(r.ResourceType)),
                EnvironmentConfigs = r.EnvironmentConfigs
                    .ToDictionary(
                        ec => ec.EnvironmentName,
                        ec => (IReadOnlyDictionary<string, string>)ec.Properties)
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
            })
            .ToList();

        var namingContext = new NamingContext
        {
            DefaultTemplate = config.NamingContext.DefaultTemplate,
            ResourceTemplates = config.NamingContext.ResourceTemplates,
            ResourceAbbreviations = ResourceAbbreviationCatalog.GetAll(),
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
            PipelineVariableGroups = MergeVariableGroups(projectVariableGroups, config.PipelineVariableGroups),
        };

        var result = pipelineGenerationEngine.Generate(generationRequest, config.Name);

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var fileUris = new Dictionary<string, Uri>();

        foreach (var (path, content) in result.Files)
        {
            var uri = await artifactService.UploadArtifactAsync(
                "pipeline", command.InfrastructureConfigId, timestamp, path, content);
            fileUris[path] = uri;
        }

        return new GeneratePipelineResult(fileUris);
    }

    private static string GetResourceTypeName(string azureResourceType) =>
        AzureResourceTypes.GetFriendlyName(azureResourceType);

    /// <summary>
    /// Merges project-level and config-level variable groups.
    /// Config-level groups take precedence when a group name already exists at project level.
    /// </summary>
    private static List<PipelineVariableGroupDefinition> MergeVariableGroups(
        List<PipelineVariableGroupDefinition> projectGroups,
        IReadOnlyList<PipelineVariableGroupReadModel> configGroups)
    {
        var merged = new Dictionary<string, PipelineVariableGroupDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var pg in projectGroups)
            merged[pg.GroupName] = pg;

        foreach (var cg in configGroups)
        {
            merged[cg.GroupName] = new PipelineVariableGroupDefinition
            {
                GroupName = cg.GroupName,
                Mappings = cg.Mappings.Select(m => new PipelineVariableMappingDefinition
                {
                    PipelineVariableName = m.PipelineVariableName,
                    BicepParameterName = m.BicepParameterName,
                }).ToList(),
            };
        }

        return merged.Values.ToList();
    }
}

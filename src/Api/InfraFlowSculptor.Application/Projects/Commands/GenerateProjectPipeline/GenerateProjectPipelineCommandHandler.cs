using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectPipeline;

/// <summary>Handles the <see cref="GenerateProjectPipelineCommand"/>.</summary>
public sealed class GenerateProjectPipelineCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IInfrastructureConfigReadRepository configReadRepository,
    PipelineGenerationEngine pipelineGenerationEngine,
    IBlobService blobService)
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

        // 2. Load all configurations for this project
        var configs = await configReadRepository.GetAllByProjectIdWithResourcesAsync(
            command.ProjectId.Value, cancellationToken);

        if (configs.Count == 0)
            return Errors.Project.NoConfigurationsError();

        // 3. Load project-level pipeline variable groups
        var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
            command.ProjectId, cancellationToken);

        var projectVariableGroups = project?.PipelineVariableGroups.ToList() ?? [];

        // 4. Generate pipeline YAML per config (mono-repo mode: no per-config variables)
        var perConfigResults = new Dictionary<string, PipelineGenerationResult>();

        foreach (var config in configs)
        {
            var generationRequest = BuildGenerationRequest(config, projectVariableGroups);
            var result = pipelineGenerationEngine.Generate(generationRequest, config.Name, isMonoRepo: true);
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
        var assembled = MonoRepoPipelineAssembler.Assemble(perConfigResults, environments);

        // 7. Upload to blob storage
        var prefix = $"pipeline/project/{command.ProjectId.Value}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

        var commonFileUris = new Dictionary<string, Uri>();
        foreach (var (path, content) in assembled.CommonFiles)
        {
            var uri = await blobService.UploadContentAsync(
                $"{prefix}/.azuredevops/{path}", content, "text/plain");
            commonFileUris[$".azuredevops/{path}"] = uri;
        }

        var configFileUris = new Dictionary<string, IReadOnlyDictionary<string, Uri>>();
        foreach (var (configName, files) in assembled.ConfigFiles)
        {
            var uris = new Dictionary<string, Uri>();
            foreach (var (path, content) in files)
            {
                var uri = await blobService.UploadContentAsync(
                    $"{prefix}/{configName}/{path}", content, "text/plain");
                uris[path] = uri;
            }
            configFileUris[configName] = uris;
        }

        return new GenerateProjectPipelineResult(commonFileUris, configFileUris);
    }

    private static GenerationRequest BuildGenerationRequest(
        InfrastructureConfigReadModel config,
        List<Domain.ProjectAggregate.Entities.ProjectPipelineVariableGroup> projectVariableGroups)
    {
        var resources = config.ResourceGroups
            .SelectMany(rg => rg.Resources.Select(r => new ResourceDefinition
            {
                Name = r.Name,
                Type = r.ResourceType,
                ResourceGroupName = rg.Name,
                Sku = r.Properties.GetValueOrDefault("sku", string.Empty),
                Properties = r.Properties,
                ResourceAbbreviation = GetResourceAbbreviation(r.ResourceType),
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
                Tags = e.Tags,
            })
            .ToList();

        var namingContext = new NamingContext
        {
            DefaultTemplate = config.NamingContext.DefaultTemplate,
            ResourceTemplates = config.NamingContext.ResourceTemplates,
            ResourceAbbreviations = ResourceAbbreviationCatalog.GetAll(),
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
                        BicepParameterName = s.Name,
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
        };
    }

    private static string GetResourceAbbreviation(string azureResourceType)
    {
        var typeName = AzureResourceTypes.GetFriendlyName(azureResourceType);
        return ResourceAbbreviationCatalog.GetAbbreviation(typeName);
    }

}

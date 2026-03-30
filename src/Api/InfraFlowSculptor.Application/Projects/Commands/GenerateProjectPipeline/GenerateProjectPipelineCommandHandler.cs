using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectPipeline;

/// <summary>Handles the <see cref="GenerateProjectPipelineCommand"/>.</summary>
public sealed class GenerateProjectPipelineCommandHandler(
    IProjectAccessService accessService,
    IInfrastructureConfigReadRepository configReadRepository,
    PipelineGenerationEngine pipelineGenerationEngine,
    IBlobService blobService)
    : IRequestHandler<GenerateProjectPipelineCommand, ErrorOr<GenerateProjectPipelineResult>>
{
    /// <summary>
    /// Maps Azure resource type strings to their simple type names used in naming template lookups.
    /// </summary>
    private static readonly Dictionary<string, string> ResourceTypeNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Microsoft.KeyVault/vaults"] = "KeyVault",
        ["Microsoft.Cache/Redis"] = "RedisCache",
        ["Microsoft.Storage/storageAccounts"] = "StorageAccount",
        ["Microsoft.Web/serverfarms"] = "AppServicePlan",
        ["Microsoft.Web/sites"] = "WebApp",
        ["Microsoft.Web/sites/functionapp"] = "FunctionApp",
        ["Microsoft.ManagedIdentity/userAssignedIdentities"] = "UserAssignedIdentity",
        ["Microsoft.AppConfiguration/configurationStores"] = "AppConfiguration",
        ["Microsoft.App/managedEnvironments"] = "ContainerAppEnvironment",
        ["Microsoft.App/containerApps"] = "ContainerApp",
        ["Microsoft.OperationalInsights/workspaces"] = "LogAnalyticsWorkspace",
        ["Microsoft.Insights/components"] = "ApplicationInsights",
        ["Microsoft.DocumentDB/databaseAccounts"] = "CosmosDb",
        ["Microsoft.Sql/servers"] = "SqlServer",
        ["Microsoft.Sql/servers/databases"] = "SqlDatabase",
    };

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
            return Error.NotFound(
                "Project.NoConfigurations",
                "No infrastructure configurations found for this project.");

        // 3. Generate pipeline YAML per config
        var perConfigResults = new Dictionary<string, PipelineGenerationResult>();

        foreach (var config in configs)
        {
            var generationRequest = BuildGenerationRequest(config);
            var result = pipelineGenerationEngine.Generate(generationRequest, config.Name);
            perConfigResults[config.Name] = result;
        }

        // 4. Assemble mono-repo output
        var assembled = MonoRepoPipelineAssembler.Assemble(perConfigResults);

        // 5. Upload to blob storage
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
        InfraFlowSculptor.Application.InfrastructureConfig.ReadModels.InfrastructureConfigReadModel config)
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
            })
            .ToList();

        var namingContext = new NamingContext
        {
            DefaultTemplate = config.NamingContext.DefaultTemplate,
            ResourceTemplates = config.NamingContext.ResourceTemplates,
            ResourceAbbreviations = ResourceAbbreviationCatalog.GetAll(),
        };

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
        };
    }

    private static string GetResourceAbbreviation(string azureResourceType)
    {
        var typeName = ResourceTypeNames.GetValueOrDefault(azureResourceType, azureResourceType);
        return ResourceAbbreviationCatalog.GetAbbreviation(typeName);
    }
}

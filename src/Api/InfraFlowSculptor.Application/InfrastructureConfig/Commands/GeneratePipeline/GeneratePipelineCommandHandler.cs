using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GeneratePipeline;

/// <summary>Handles the <see cref="GeneratePipelineCommand"/>.</summary>
public sealed class GeneratePipelineCommandHandler(
    IInfrastructureConfigReadRepository configRepository,
    PipelineGenerationEngine pipelineGenerationEngine,
    IGeneratedArtifactService artifactService)
    : IRequestHandler<GeneratePipelineCommand, ErrorOr<GeneratePipelineResult>>
{
    public async Task<ErrorOr<GeneratePipelineResult>> Handle(
        GeneratePipelineCommand command,
        CancellationToken cancellationToken)
    {
        var config = await configRepository.GetByIdWithResourcesAsync(
            command.InfrastructureConfigId, cancellationToken);

        if (config is null)
            return Error.NotFound(
                "InfrastructureConfig.NotFound",
                $"Infrastructure configuration '{command.InfrastructureConfigId}' was not found.");

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
        };

        var result = pipelineGenerationEngine.Generate(generationRequest);

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

    /// <summary>
    /// Maps Azure resource type strings to their simple type names.
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

    private static string GetResourceTypeName(string azureResourceType) =>
        ResourceTypeNames.GetValueOrDefault(azureResourceType, azureResourceType);
}

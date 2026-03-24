using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.BicepGeneration.Models;
using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBicep;

public sealed class GenerateBicepCommandHandler(
    IInfrastructureConfigReadRepository configRepository,
    BicepGenerationEngine bicepGenerationEngine,
    IBlobService blobService)
    : IRequestHandler<GenerateBicepCommand, ErrorOr<GenerateBicepResult>>
{
    /// <summary>
    /// The subdirectory name where Bicep parameter files are stored.
    /// </summary>
    private const string ParametersDirectory = "parameters";

    /// <summary>
    /// The file extension for Bicep parameter files.
    /// </summary>
    private const string BicepParameterExtension = ".bicepparam";

    /// <summary>
    /// Maps Azure resource type strings to their simple type names used in naming template lookups
    /// and abbreviation resolution via <see cref="ResourceAbbreviationCatalog"/>.
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

    public async Task<ErrorOr<GenerateBicepResult>> Handle(
        GenerateBicepCommand command,
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
        };

        var result = bicepGenerationEngine.Generate(generationRequest);

        var prefix = $"bicep/{config.Id}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

        // Upload types.bicep
        await blobService.UploadContentAsync(
            $"{prefix}/types.bicep",
            result.TypesBicep,
            "text/plain");

        // Upload functions.bicep
        await blobService.UploadContentAsync(
            $"{prefix}/functions.bicep",
            result.FunctionsBicep,
            "text/plain");

        // Upload main.bicep
        var mainBicepUri = await blobService.UploadContentAsync(
            $"{prefix}/main.bicep",
            result.MainBicep,
            "text/plain");

        var parameterUris = new Dictionary<string, Uri>();
        foreach (var (fileName, content) in result.EnvironmentParameterFiles)
        {
            var destinationPath = ResolveArtifactPath(prefix, fileName);
            var paramUri = await blobService.UploadContentAsync(
                destinationPath,
                content,
                "text/plain");
            parameterUris[$"{ParametersDirectory}/{fileName}"] = paramUri;
        }

        var moduleUris = new Dictionary<string, Uri>();
        foreach (var (path, content) in result.ModuleFiles)
        {
            var moduleUri = await blobService.UploadContentAsync(
                $"{prefix}/{path}",
                content,
                "text/plain");
            moduleUris[path] = moduleUri;
        }

        return new GenerateBicepResult(mainBicepUri, parameterUris, moduleUris);
    }

    /// <summary>
    /// Resolves the destination storage path for a Bicep artifact based on its file type.
    /// Parameter files (.bicepparam) are placed in a <c>parameters/</c> subdirectory;
    /// other artifacts remain in the base prefix directory.
    /// </summary>
    /// <param name="prefix">The base storage prefix (e.g., "bicep/{configId}/{timestamp}").</param>
    /// <param name="fileName">The name of the file to upload.</param>
    /// <returns>The full destination path for the artifact in blob storage.</returns>
    private static string ResolveArtifactPath(string prefix, string fileName) =>
        fileName.EndsWith(BicepParameterExtension, StringComparison.OrdinalIgnoreCase)
            ? $"{prefix}/{ParametersDirectory}/{fileName}"
            : $"{prefix}/{fileName}";

    /// <summary>
    /// Resolves the resource abbreviation from the Azure resource type string.
    /// </summary>
    private static string GetResourceAbbreviation(string azureResourceType)
    {
        var typeName = ResourceTypeNames.GetValueOrDefault(azureResourceType, azureResourceType);
        return ResourceAbbreviationCatalog.GetAbbreviation(typeName);
    }
}

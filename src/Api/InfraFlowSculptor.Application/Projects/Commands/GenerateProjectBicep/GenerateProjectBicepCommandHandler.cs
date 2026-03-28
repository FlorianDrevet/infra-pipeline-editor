using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ResourceOutputs;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBicep;

/// <summary>Handles the <see cref="GenerateProjectBicepCommand"/>.</summary>
public sealed class GenerateProjectBicepCommandHandler(
    IProjectAccessService accessService,
    IInfrastructureConfigReadRepository configReadRepository,
    BicepGenerationEngine bicepGenerationEngine,
    IBlobService blobService)
    : IRequestHandler<GenerateProjectBicepCommand, ErrorOr<GenerateProjectBicepResult>>
{
    /// <summary>The subdirectory name where Bicep parameter files are stored.</summary>
    private const string ParametersDirectory = "parameters";

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
    public async Task<ErrorOr<GenerateProjectBicepResult>> Handle(
        GenerateProjectBicepCommand command,
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

        // 3. Build per-config generation requests
        var configRequests = new Dictionary<string, GenerationRequest>();
        NamingContext? sharedNamingContext = null;
        var allEnvironments = new List<EnvironmentDefinition>();
        var allEnvironmentNames = new List<string>();

        foreach (var config in configs)
        {
            var generationRequest = BuildGenerationRequest(config);
            configRequests[config.Name] = generationRequest;

            // Use the first config's naming context and environments as shared (they come from the project)
            if (sharedNamingContext is null)
            {
                sharedNamingContext = generationRequest.NamingContext;
                allEnvironments = generationRequest.Environments.ToList();
                allEnvironmentNames = generationRequest.EnvironmentNames.ToList();
            }
        }

        // 4. Generate mono-repo Bicep files
        var monoRepoRequest = new MonoRepoGenerationRequest
        {
            ConfigRequests = configRequests,
            NamingContext = sharedNamingContext!,
            Environments = allEnvironments,
            EnvironmentNames = allEnvironmentNames,
        };

        var result = bicepGenerationEngine.GenerateMonoRepo(monoRepoRequest);

        // 5. Upload to blob storage
        var prefix = $"bicep/project/{command.ProjectId.Value}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

        var commonFileUris = new Dictionary<string, Uri>();
        foreach (var (path, content) in result.CommonFiles)
        {
            var uri = await blobService.UploadContentAsync(
                $"{prefix}/Common/{path}", content, "text/plain");
            commonFileUris[$"Common/{path}"] = uri;
        }

        var configFileUris = new Dictionary<string, IReadOnlyDictionary<string, Uri>>();
        foreach (var (configName, files) in result.ConfigFiles)
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

        return new GenerateProjectBicepResult(commonFileUris, configFileUris);
    }

    private static GenerationRequest BuildGenerationRequest(
        InfrastructureConfig.ReadModels.InfrastructureConfigReadModel config)
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
            })
            .ToList();

        var namingContext = new NamingContext
        {
            DefaultTemplate = config.NamingContext.DefaultTemplate,
            ResourceTemplates = config.NamingContext.ResourceTemplates,
            ResourceAbbreviations = ResourceAbbreviationCatalog.GetAll(),
        };

        var roleAssignments = config.RoleAssignments
            .Select(ra =>
            {
                var targetTypeName = GetResourceTypeName(ra.TargetResourceType);
                var sourceTypeName = GetResourceTypeName(ra.SourceResourceType);
                var roleDef = AzureRoleDefinitionCatalog.GetForResourceType(targetTypeName)
                    .FirstOrDefault(r => r.Id.Equals(ra.RoleDefinitionId, StringComparison.OrdinalIgnoreCase));

                return new RoleAssignmentDefinition
                {
                    SourceResourceName = ra.SourceResourceName,
                    SourceResourceType = ra.SourceResourceType,
                    SourceResourceTypeName = sourceTypeName,
                    SourceResourceGroupName = ra.SourceResourceGroupName,
                    TargetResourceName = ra.TargetResourceName,
                    TargetResourceType = ra.TargetResourceType,
                    TargetResourceGroupName = ra.TargetResourceGroupName,
                    ManagedIdentityType = ra.ManagedIdentityType,
                    RoleDefinitionId = ra.RoleDefinitionId,
                    RoleDefinitionName = roleDef?.Name ?? ra.RoleDefinitionId,
                    RoleDefinitionDescription = roleDef?.Description ?? string.Empty,
                    ServiceCategory = RoleAssignmentModuleTemplates.GetServiceCategory(targetTypeName),
                    TargetResourceTypeName = targetTypeName,
                    TargetResourceAbbreviation = GetResourceAbbreviation(ra.TargetResourceType),
                    UserAssignedIdentityName = ra.UserAssignedIdentityName,
                    UserAssignedIdentityResourceGroupName = ra.UserAssignedIdentityResourceGroupName,
                    IsTargetCrossConfig = ra.IsTargetCrossConfig,
                };
            })
            .ToList();

        var appSettingDefinitions = config.AppSettings
            .Select(s =>
            {
                var sourceTypeName = s.SourceResourceType is not null
                    ? GetResourceTypeName(s.SourceResourceType)
                    : null;

                string? bicepExpression = null;
                if (sourceTypeName is not null && s.SourceOutputName is not null)
                {
                    var outputDef = ResourceOutputCatalog.FindOutput(sourceTypeName, s.SourceOutputName);
                    bicepExpression = outputDef?.BicepExpression;
                }

                // Detect sensitive output exported to KV: has both source + KV references
                var isSensitiveExport = s.IsKeyVaultReference
                    && s.SourceResourceId is not null
                    && s.SourceOutputName is not null;

                return new AppSettingDefinition
                {
                    Name = s.Name,
                    StaticValue = s.StaticValue,
                    SourceResourceName = s.SourceResourceName,
                    SourceOutputName = s.SourceOutputName,
                    SourceResourceTypeName = sourceTypeName,
                    TargetResourceName = s.ResourceName,
                    IsOutputReference = s.IsOutputReference,
                    SourceOutputBicepExpression = bicepExpression,
                    IsKeyVaultReference = s.IsKeyVaultReference,
                    KeyVaultResourceName = s.KeyVaultResourceName,
                    SecretName = s.SecretName,
                    IsSourceCrossConfig = s.IsSourceCrossConfig,
                    SourceResourceGroupName = s.SourceResourceGroupName,
                    IsSensitiveOutputExportedToKeyVault = isSensitiveExport,
                };
            })
            .ToList();

        var existingResourceReferences = config.CrossConfigReferences
            .Select(ccRef =>
            {
                var targetTypeName = GetResourceTypeName(ccRef.TargetResourceType);
                return new ExistingResourceReference
                {
                    ResourceName = ccRef.TargetResourceName,
                    ResourceTypeName = targetTypeName,
                    ResourceType = ccRef.TargetResourceType,
                    ResourceGroupName = ccRef.TargetResourceGroupName,
                    ResourceAbbreviation = ccRef.TargetResourceAbbreviation,
                    SourceConfigName = ccRef.TargetConfigName,
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
            RoleAssignments = roleAssignments,
            AppSettings = appSettingDefinitions,
            ExistingResourceReferences = existingResourceReferences,
        };
    }

    private static string GetResourceAbbreviation(string azureResourceType)
    {
        var typeName = ResourceTypeNames.GetValueOrDefault(azureResourceType, azureResourceType);
        return ResourceAbbreviationCatalog.GetAbbreviation(typeName);
    }

    private static string GetResourceTypeName(string azureResourceType) =>
        ResourceTypeNames.GetValueOrDefault(azureResourceType, azureResourceType);
}

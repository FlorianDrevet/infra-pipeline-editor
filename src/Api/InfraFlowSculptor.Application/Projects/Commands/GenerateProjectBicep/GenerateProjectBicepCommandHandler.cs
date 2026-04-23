using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ResourceOutputs;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBicep;

/// <summary>Handles the <see cref="GenerateProjectBicepCommand"/>.</summary>
public sealed class GenerateProjectBicepCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IInfrastructureConfigRepository configRepository,
    IInfrastructureConfigReadRepository configReadRepository,
    BicepGenerationEngine bicepGenerationEngine,
    IBlobService blobService)
    : ICommandHandler<GenerateProjectBicepCommand, GenerateProjectBicepResult>
{
    /// <summary>The subdirectory name where Bicep parameter files are stored.</summary>
    private const string ParametersDirectory = "parameters";



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
            return Errors.Project.NoConfigurationsError();

        // 2.bis Ambiguity gate: reject project-level generate-all for heterogeneous multi-repo topologies.
        // Uses domain aggregates to access RepositoryBinding (not exposed by the read model).
        var project = await projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var domainConfigs = await configRepository.GetByProjectIdAsync(command.ProjectId, cancellationToken);
        if (!project.CanGenerateAllFromProjectLevel(domainConfigs))
            return Errors.GitRouting.AmbiguousProjectLevelGeneration;

        // 3. Build per-config generation requests
        var configRequests = new Dictionary<string, GenerationRequest>();
        NamingContext? sharedNamingContext = null;
        var allEnvironments = new List<EnvironmentDefinition>();
        var allEnvironmentNames = new List<string>();

        foreach (var config in configs)
        {
            var generationRequest = BuildGenerationRequest(config);
            configRequests[PathSanitizer.Sanitize(config.Name)] = generationRequest;

            // Use the first config's naming context and environments as shared (they come from the project)
            if (sharedNamingContext is null)
            {
                sharedNamingContext = generationRequest.NamingContext;
                allEnvironments = generationRequest.Environments.ToList();
                allEnvironmentNames = generationRequest.EnvironmentNames.ToList();
            }
        }

        // 4. Generate mono-repo Bicep files.
        // For SplitInfraCode projects, shared files are emitted at the repository root
        // (no Common/ wrapper) since the infra repo only holds bicep + pipelines.
        // For AllInOne, shared files stay namespaced under Common/.
        var flattenShared = project.LayoutPreset?.Value == Domain.ProjectAggregate.ValueObjects.LayoutPresetEnum.SplitInfraCode;

        var monoRepoRequest = new MonoRepoGenerationRequest
        {
            ConfigRequests = configRequests,
            NamingContext = sharedNamingContext!,
            Environments = allEnvironments,
            EnvironmentNames = allEnvironmentNames,
            FlattenShared = flattenShared,
        };

        var result = bicepGenerationEngine.GenerateMonoRepo(monoRepoRequest);

        // 5. Upload to blob storage
        var prefix = $"bicep/project/{command.ProjectId.Value}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var sharedPathSegment = flattenShared ? string.Empty : "Common/";

        var commonFileUris = new Dictionary<string, Uri>();
        foreach (var (path, content) in result.CommonFiles)
        {
            var uri = await blobService.UploadContentAsync(
                $"{prefix}/{sharedPathSegment}{path}", content, "text/plain");
            commonFileUris[$"{sharedPathSegment}{path}"] = uri;
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
        var mergedAbbreviations = MergeAbbreviations(config.NamingContext.ResourceAbbreviations);

        var resources = config.ResourceGroups
            .SelectMany(rg => rg.Resources
                .Where(r => !r.IsExisting)
                .Select(r => new ResourceDefinition
            {
                ResourceId = r.Id,
                Name = r.Name,
                Type = r.ResourceType,
                ResourceGroupName = rg.Name,
                Sku = r.Properties.GetValueOrDefault("sku", string.Empty),
                Properties = r.Properties,
                ResourceAbbreviation = GetResourceAbbreviation(r.ResourceType, mergedAbbreviations),
                EnvironmentConfigs = r.EnvironmentConfigs
                    .ToDictionary(
                        ec => ec.EnvironmentName,
                        ec => (IReadOnlyDictionary<string, string>)ec.Properties),
                AssignedUserAssignedIdentityName = r.AssignedUserAssignedIdentityName,
                CustomDomains = (r.CustomDomains ?? [])
                    .Select(cd => new CustomDomainDefinition
                    {
                        EnvironmentName = cd.EnvironmentName,
                        DomainName = cd.DomainName,
                        BindingType = cd.BindingType,
                    })
                    .ToList(),
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
            ResourceAbbreviations = mergedAbbreviations,
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
                    TargetResourceAbbreviation = GetResourceAbbreviation(ra.TargetResourceType, mergedAbbreviations),
                    UserAssignedIdentityName = ra.UserAssignedIdentityName,
                    UserAssignedIdentityResourceId = ra.UserAssignedIdentityResourceId,
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
                    StaticValue = null,
                    EnvironmentValues = s.EnvironmentValues,
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
                    SecretValueAssignment = s.SecretValueAssignment?.ToString(),
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

        var localExistingRefs = config.ResourceGroups
            .SelectMany(rg => rg.Resources
                .Where(r => r.IsExisting)
                .Select(r =>
                {
                    var typeName = GetResourceTypeName(r.ResourceType);
                    return new ExistingResourceReference
                    {
                        ResourceName = r.Name,
                        ResourceTypeName = typeName,
                        ResourceType = r.ResourceType,
                        ResourceGroupName = rg.Name,
                        ResourceAbbreviation = GetResourceAbbreviation(r.ResourceType, mergedAbbreviations),
                        SourceConfigName = string.Empty,
                    };
                }))
            .ToList();

        existingResourceReferences.AddRange(localExistingRefs);

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
            ProjectTags = config.ProjectTags,
            ConfigTags = config.ConfigTags,
        };
    }

    private static string GetResourceAbbreviation(
        string azureResourceType,
        IReadOnlyDictionary<string, string> mergedAbbreviations)
    {
        var typeName = AzureResourceTypes.GetFriendlyName(azureResourceType);
        return mergedAbbreviations.TryGetValue(typeName, out var abbr)
            ? abbr
            : ResourceAbbreviationCatalog.GetAbbreviation(typeName);
    }

    private static IReadOnlyDictionary<string, string> MergeAbbreviations(
        IReadOnlyDictionary<string, string> overrides)
    {
        var merged = new Dictionary<string, string>(ResourceAbbreviationCatalog.GetAll(), StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in overrides)
        {
            merged[key] = value;
        }

        return merged;
    }

    private static string GetResourceTypeName(string azureResourceType) =>
        AzureResourceTypes.GetFriendlyName(azureResourceType);
}

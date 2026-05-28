using System.Text.Json;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.Entities;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.KeyVaultAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.RedisCacheAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.Entities;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Entities;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate.Entities;
using InfraFlowSculptor.Domain.ApplicationInsightsAggregate.Entities;
using InfraFlowSculptor.Domain.CosmosDbAggregate;
using InfraFlowSculptor.Domain.CosmosDbAggregate.Entities;
using InfraFlowSculptor.Domain.SqlServerAggregate;
using InfraFlowSculptor.Domain.SqlServerAggregate.Entities;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.Entities;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate.Entities;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.Entities;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public sealed class InfrastructureConfigReadRepository(ProjectDbContext dbContext)
    : IInfrastructureConfigReadRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<InfrastructureConfigReadModel>> GetAllByProjectIdWithResourcesAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var projectIdVo = new ProjectId(projectId);

        var configIds = await dbContext.InfrastructureConfigs
            .AsNoTracking()
            .Where(c => c.ProjectId == projectIdVo)
            .Select(c => c.Id.Value)
            .ToListAsync(cancellationToken);

        var results = new List<InfrastructureConfigReadModel>();
        foreach (var configId in configIds)
        {
            var readModel = await GetByIdWithResourcesAsync(configId, cancellationToken);
            if (readModel is not null)
                results.Add(readModel);
        }

        return results;
    }

    public async Task<InfrastructureConfigReadModel?> GetByIdWithResourcesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var configId = new InfrastructureConfigId(id);

        var config = await dbContext.InfrastructureConfigs
            .Include(c => c.ResourceGroups)
                .ThenInclude(rg => rg.Resources)
            .Include(c => c.ResourceNamingTemplates)
            .Include(c => c.ResourceAbbreviationOverrides)
            .Include(c => c.Tags)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == configId, cancellationToken);

        if (config is null)
            return null;

        // Eager-load typed environment settings for each resource type
        var allResourceIds = config.ResourceGroups
            .SelectMany(rg => rg.Resources)
            .Select(r => r.Id)
            .ToList();

        // NOTE: These queries are sequential because EF Core's DbContext is not thread-safe.
        // Parallelization would require IDbContextFactory. Since Phase 1.3 moved diagnostics
        // out of the critical path (fire-and-forget), these sequential queries no longer block
        // the page rendering.

        var kvSettings = await dbContext.KeyVaultEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.KeyVaultId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var rcSettings = await dbContext.RedisCacheEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.RedisCacheId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var saSettings = await dbContext.StorageAccountEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.StorageAccountId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var blobContainers = await dbContext.BlobContainers
            .Where(bc => allResourceIds.Contains(bc.StorageAccountId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var storageQueues = await dbContext.StorageQueues
            .Where(queue => allResourceIds.Contains(queue.StorageAccountId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var storageTables = await dbContext.StorageTables
            .Where(table => allResourceIds.Contains(table.StorageAccountId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var storageCorsRules = await dbContext.StorageAccountCorsRules
            .Where(cr => allResourceIds.Contains(cr.StorageAccountId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var lifecycleRules = await dbContext.BlobLifecycleRules
            .Where(lr => allResourceIds.Contains(lr.StorageAccountId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var aspSettings = await dbContext.AppServicePlanEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.AppServicePlanId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var waSettings = await dbContext.WebAppEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.WebAppId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var faSettings = await dbContext.FunctionAppEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.FunctionAppId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var acSettings = await dbContext.AppConfigurationEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.AppConfigurationId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var caeSettings = await dbContext.ContainerAppEnvironmentEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.ContainerAppEnvironmentId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var caSettings = await dbContext.ContainerAppEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.ContainerAppId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var lawSettings = await dbContext.LogAnalyticsWorkspaceEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.LogAnalyticsWorkspaceId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var aiSettings = await dbContext.ApplicationInsightsEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.ApplicationInsightsId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var cosmosSettings = await dbContext.CosmosDbEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.CosmosDbId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var sqlServerSettings = await dbContext.SqlServerEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.SqlServerId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var sqlDbSettings = await dbContext.SqlDatabaseEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.SqlDatabaseId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var sbSettings = await dbContext.ServiceBusNamespaceEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.ServiceBusNamespaceId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var crSettings = await dbContext.ContainerRegistryEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.ContainerRegistryId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var ehSettings = await dbContext.EventHubNamespaceEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.EventHubNamespaceId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var docIntSettings = await dbContext.DocumentIntelligenceEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.DocumentIntelligenceId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // â”€â”€ Load custom domains for all resources in this config â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var customDomains = await dbContext.CustomDomains
            .Where(cd => allResourceIds.Contains(cd.ResourceId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // â”€â”€ Load role assignments for all resources in this config â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var roleAssignments = await dbContext.RoleAssignments
            .Where(ra => allResourceIds.Contains(ra.SourceResourceId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // â”€â”€ Load app settings for all resources in this config â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var appSettings = await dbContext.AppSettings
            .Include(s => s.EnvironmentValues)
            .Where(s => allResourceIds.Contains(s.ResourceId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // â”€â”€ Load secure parameter mappings for all resources in this config â”€
        var secureParameterMappings = await dbContext.SecureParameterMappings
            .Where(m => allResourceIds.Contains(m.ResourceId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Build a flat lookup of all resources by ID across all resource groups
        var allResources = config.ResourceGroups
            .SelectMany(rg => rg.Resources.Select(r => (Resource: r, ResourceGroup: rg)))
            .ToDictionary(x => x.Resource.Id);

        // Pre-load external target resources (cross-config) that are not in allResources
        var externalTargetIds = roleAssignments
            .Select(ra => ra.TargetResourceId)
            .Where(id => !allResources.ContainsKey(id))
            .Distinct()
            .ToList();

        var externalTargets = await LoadExternalTargetsAsync(externalTargetIds, cancellationToken);

        var roleAssignmentReadModels = BuildRoleAssignmentReadModels(roleAssignments, allResources, externalTargets);
        var mappingContext = new ResourceMappingContext(
            kvSettings,
            rcSettings,
            saSettings,
            blobContainers,
            storageQueues,
            storageTables,
            storageCorsRules,
            lifecycleRules,
            aspSettings,
            waSettings,
            faSettings,
            acSettings,
            caeSettings,
            caSettings,
            lawSettings,
            aiSettings,
            cosmosSettings,
            sqlServerSettings,
            sqlDbSettings,
            sbSettings,
            crSettings,
            ehSettings,
            docIntSettings);

        var resourceGroups = BuildResourceGroupReadModels(
            config.ResourceGroups,
            allResources,
            customDomains,
            r => MapResource(r, mappingContext));

        // â”€â”€ Load parent project for environments and naming context â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var project = await dbContext.Projects
            .Include(p => p.ResourceNamingTemplates)
            .Include(p => p.ResourceAbbreviations)
            .Include(p => p.Tags)
            .Include(p => p.EnvironmentDefinitions)
                .ThenInclude(e => e.Tags)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == config.ProjectId, cancellationToken);

        var environments = BuildEnvironmentList(project);
        var namingContext = BuildNamingContext(config, project);

        // â”€â”€ Load pipeline variable groups for VG name resolution â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var projectVarGroups = project is not null
            ? await dbContext.ProjectPipelineVariableGroups
                .Where(g => g.ProjectId == project.Id)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
            : [];

        var varGroupLookup = projectVarGroups.ToDictionary(g => g.Id, g => g.GroupName);

        var appSettingReadModels = BuildAppSettingReadModels(appSettings, allResources, varGroupLookup);

        // â”€â”€ Load cross-config resource references â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var crossConfigReferences = await dbContext.CrossConfigResourceReferences
            .Where(r => r.InfraConfigId == configId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var crossConfigRefReadModels = await BuildCrossConfigReferenceReadModelsAsync(
            crossConfigReferences, namingContext, cancellationToken);

        var crossConfigRefLookup = crossConfigRefReadModels
            .ToDictionary(r => r.TargetResourceId);

        var enrichedRoleAssignments = EnrichRoleAssignmentsWithCrossConfig(
            roleAssignmentReadModels, crossConfigReferences, crossConfigRefLookup);

        var enrichedAppSettings = EnrichAppSettingsWithCrossConfig(
            appSettingReadModels, crossConfigRefLookup);

        var projectTags = project?.Tags
            .ToDictionary(t => t.Name, t => t.Value)
            ?? new Dictionary<string, string>();

        var configTags = config.Tags
            .ToDictionary(t => t.Name, t => t.Value);

        var secureParamReadModels = BuildSecureParameterReadModels(
            secureParameterMappings, allResources, varGroupLookup);

        return new InfrastructureConfigReadModel(
            config.Id.Value,
            config.Name.Value,
            config.ProjectId.Value,
            resourceGroups,
            environments,
            namingContext,
            enrichedRoleAssignments,
            enrichedAppSettings,
            crossConfigRefReadModels,
            projectTags,
            configTags,
            config.AppPipelineMode.ToString(),
            secureParamReadModels);
    }

    /// <summary>
    /// Resolves the environment list from the parent project.
    /// </summary>
    private static List<EnvironmentDefinitionReadModel> BuildEnvironmentList(
        Project? project)
    {
        if (project is null)
        {
            return [];
        }

        return project.EnvironmentDefinitions.Select(e =>
            new EnvironmentDefinitionReadModel(
                e.Id.Value,
                e.Name.Value,
                e.ShortName.Value,
                MapLocation(e.Location),
                e.Prefix.Value,
                e.Suffix.Value,
                e.AzureResourceManagerConnection,
                e.SubscriptionId.Value == Guid.Empty ? null : e.SubscriptionId.Value.ToString(),
                e.Tags.ToDictionary(t => t.Name, t => t.Value))).ToList();
    }

    /// <summary>
    /// Resolves the effective naming context based on inheritance settings.
    /// </summary>
    private static NamingContextReadModel BuildNamingContext(
        InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig config,
        Project? project)
    {
        // When using project naming conventions (default), read from project
        if (config.UseProjectNamingConventions && project is not null)
        {
            return new NamingContextReadModel(
                project.DefaultNamingTemplate?.Value,
                project.ResourceNamingTemplates.ToDictionary(
                    t => t.ResourceType,
                    t => t.Template.Value),
                project.ResourceAbbreviations.ToDictionary(
                    a => a.ResourceType,
                    a => a.Abbreviation));
        }

        // Otherwise, read from the config itself (if overridden)
        return new NamingContextReadModel(
            config.DefaultNamingTemplate?.Value,
            config.ResourceNamingTemplates.ToDictionary(
                t => t.ResourceType,
                t => t.Template.Value),
            config.ResourceAbbreviationOverrides.ToDictionary(
                a => a.ResourceType,
                a => a.Abbreviation));
    }

    /// <summary>
    /// Maps an <see cref="AzureResource"/> to its read model using typed environment settings.
    /// Returns <c>null</c> for resource types that are not yet supported by the generator.
    /// </summary>
    private sealed record ResourceMappingContext(
        IReadOnlyList<KeyVaultEnvironmentSettings> KvSettings,
        IReadOnlyList<RedisCacheEnvironmentSettings> RcSettings,
        IReadOnlyList<StorageAccountEnvironmentSettings> SaSettings,
        IReadOnlyList<BlobContainer> BlobContainers,
        IReadOnlyList<StorageQueue> StorageQueues,
        IReadOnlyList<StorageTable> StorageTables,
        IReadOnlyList<CorsRule> StorageCorsRules,
        IReadOnlyList<BlobLifecycleRule> LifecycleRules,
        IReadOnlyList<AppServicePlanEnvironmentSettings> AspSettings,
        IReadOnlyList<WebAppEnvironmentSettings> WaSettings,
        IReadOnlyList<FunctionAppEnvironmentSettings> FaSettings,
        IReadOnlyList<AppConfigurationEnvironmentSettings> AcSettings,
        IReadOnlyList<ContainerAppEnvironmentEnvironmentSettings> CaeSettings,
        IReadOnlyList<ContainerAppEnvironmentSettings> CaSettings,
        IReadOnlyList<LogAnalyticsWorkspaceEnvironmentSettings> LawSettings,
        IReadOnlyList<ApplicationInsightsEnvironmentSettings> AiSettings,
        IReadOnlyList<CosmosDbEnvironmentSettings> CosmosSettings,
        IReadOnlyList<SqlServerEnvironmentSettings> SqlServerSettings,
        IReadOnlyList<SqlDatabaseEnvironmentSettings> SqlDbSettings,
        IReadOnlyList<ServiceBusNamespaceEnvironmentSettings> SbSettings,
        IReadOnlyList<ContainerRegistryEnvironmentSettings> CrSettings,
        IReadOnlyList<EventHubNamespaceEnvironmentSettings> EhSettings,
        IReadOnlyList<DocumentIntelligenceEnvironmentSettings> DocIntSettings);

    private static AzureResourceReadModel? MapResource(
        AzureResource resource,
        ResourceMappingContext context)
    {
        var kvSettings = context.KvSettings;
        var rcSettings = context.RcSettings;
        var saSettings = context.SaSettings;
        var blobContainers = context.BlobContainers;
        var storageQueues = context.StorageQueues;
        var storageTables = context.StorageTables;
        var storageCorsRules = context.StorageCorsRules;
        var lifecycleRules = context.LifecycleRules;
        var aspSettings = context.AspSettings;
        var waSettings = context.WaSettings;
        var faSettings = context.FaSettings;
        var acSettings = context.AcSettings;
        var caeSettings = context.CaeSettings;
        var caSettings = context.CaSettings;
        var lawSettings = context.LawSettings;
        var aiSettings = context.AiSettings;
        var cosmosSettings = context.CosmosSettings;
        var sqlServerSettings = context.SqlServerSettings;
        var sqlDbSettings = context.SqlDbSettings;
        var sbSettings = context.SbSettings;
        var crSettings = context.CrSettings;
        var ehSettings = context.EhSettings;

        return resource switch
        {
            KeyVault kv => new AzureResourceReadModel(
                kv.Id.Value,
                kv.Name.Value,
                MapLocation(kv.Location),
                AzureResourceTypes.ArmTypes.KeyVaultType,
                new Dictionary<string, string>
                {
                    ["enableRbacAuthorization"] = kv.EnableRbacAuthorization.ToString().ToLower(),
                    ["enabledForDeployment"] = kv.EnabledForDeployment.ToString().ToLower(),
                    ["enabledForDiskEncryption"] = kv.EnabledForDiskEncryption.ToString().ToLower(),
                    ["enabledForTemplateDeployment"] = kv.EnabledForTemplateDeployment.ToString().ToLower(),
                    ["enablePurgeProtection"] = kv.EnablePurgeProtection.ToString().ToLower(),
                    ["enableSoftDelete"] = kv.EnableSoftDelete.ToString().ToLower()
                },
                kvSettings
                    .Where(es => es.KeyVaultId == kv.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            RedisCache rc => new AzureResourceReadModel(
                rc.Id.Value,
                rc.Name.Value,
                MapLocation(rc.Location),
                AzureResourceTypes.ArmTypes.RedisCacheType,
                new Dictionary<string, string>(),
                rcSettings
                    .Where(es => es.RedisCacheId == rc.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            StorageAccount sa => new AzureResourceReadModel(
                sa.Id.Value,
                sa.Name.Value,
                MapLocation(sa.Location),
                AzureResourceTypes.ArmTypes.StorageAccountType,
                new Dictionary<string, string>
                {
                    ["kind"] = sa.Kind.Value.ToString(),
                    ["accessTier"] = sa.AccessTier.Value.ToString(),
                    ["allowBlobPublicAccess"] = sa.AllowBlobPublicAccess.ToString().ToLower(),
                    ["supportsHttpsTrafficOnly"] = sa.EnableHttpsTrafficOnly.ToString().ToLower(),
                    ["blobContainerNames"] = JsonSerializer.Serialize(blobContainers
                        .Where(bc => bc.StorageAccountId == sa.Id)
                        .Select(bc => bc.Name)
                        .ToList()),
                    ["queueNames"] = JsonSerializer.Serialize(storageQueues
                        .Where(queue => queue.StorageAccountId == sa.Id)
                        .Select(queue => queue.Name)
                        .ToList()),
                    ["storageTableNames"] = JsonSerializer.Serialize(storageTables
                        .Where(table => table.StorageAccountId == sa.Id)
                        .Select(table => table.Name)
                        .ToList()),
                    ["corsRules"] = JsonSerializer.Serialize(storageCorsRules
                        .Where(rule => rule.StorageAccountId == sa.Id && rule.ServiceType == new CorsServiceType(CorsServiceType.Service.Blob))
                        .Select(rule => new
                        {
                            allowedOrigins = rule.AllowedOrigins,
                            allowedMethods = rule.AllowedMethods,
                            allowedHeaders = rule.AllowedHeaders,
                            exposedHeaders = rule.ExposedHeaders,
                            maxAgeInSeconds = rule.MaxAgeInSeconds
                        })
                        .ToList()),
                    ["tableCorsRules"] = JsonSerializer.Serialize(storageCorsRules
                        .Where(rule => rule.StorageAccountId == sa.Id && rule.ServiceType == new CorsServiceType(CorsServiceType.Service.Table))
                        .Select(rule => new
                        {
                            allowedOrigins = rule.AllowedOrigins,
                            allowedMethods = rule.AllowedMethods,
                            allowedHeaders = rule.AllowedHeaders,
                            exposedHeaders = rule.ExposedHeaders,
                            maxAgeInSeconds = rule.MaxAgeInSeconds
                        })
                        .ToList()),
                    ["lifecycleRules"] = JsonSerializer.Serialize(lifecycleRules
                        .Where(lr => lr.StorageAccountId == sa.Id)
                        .Select(lr => new
                        {
                            ruleName = lr.RuleName,
                            containerNames = lr.ContainerNames,
                            timeToLiveInDays = lr.TimeToLiveInDays
                        })
                        .ToList()),
                    ["minimumTlsVersion"] = sa.MinimumTlsVersion.Value.ToString() switch
                    {
                        "Tls10" => "TLS1_0",
                        "Tls11" => "TLS1_1",
                        "Tls12" => "TLS1_2",
                        var v => v
                    }
                },
                saSettings
                    .Where(es => es.StorageAccountId == sa.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            AppServicePlan asp => new AzureResourceReadModel(
                asp.Id.Value,
                asp.Name.Value,
                MapLocation(asp.Location),
                AzureResourceTypes.ArmTypes.AppServicePlanType,
                new Dictionary<string, string>
                {
                    ["osType"] = asp.OsType.Value.ToString()
                },
                aspSettings
                    .Where(es => es.AppServicePlanId == asp.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            WebApp wa => new AzureResourceReadModel(
                wa.Id.Value,
                wa.Name.Value,
                MapLocation(wa.Location),
                AzureResourceTypes.ArmTypes.WebAppType,
                new Dictionary<string, string>
                {
                    ["runtimeStack"] = wa.RuntimeStack.Value.ToString().ToLower(),
                    ["runtimeVersion"] = wa.RuntimeVersion,
                    ["alwaysOn"] = wa.AlwaysOn.ToString().ToLower(),
                    ["httpsOnly"] = wa.HttpsOnly.ToString().ToLower(),
                    ["appServicePlanId"] = wa.AppServicePlanId.Value.ToString(),
                    ["deploymentMode"] = wa.DeploymentMode.Value.ToString(),
                    ["containerRegistryId"] = wa.ContainerRegistryId?.Value.ToString() ?? "",
                    ["acrAuthMode"] = wa.AcrAuthMode?.Value.ToString() ?? "",
                    ["acrPullIdentityId"] = wa.AcrPullIdentityId?.Value.ToString() ?? "",
                    ["dockerImageName"] = wa.DockerImageName ?? "",
                    ["dockerImageValidated"] = wa.DockerImageValidated.ToString().ToLowerInvariant()
                },
                waSettings
                    .Where(es => es.WebAppId == wa.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            FunctionApp fa => new AzureResourceReadModel(
                fa.Id.Value,
                fa.Name.Value,
                MapLocation(fa.Location),
                AzureResourceTypes.ArmTypes.FunctionAppType,
                new Dictionary<string, string>
                {
                    ["runtimeStack"] = fa.RuntimeStack.Value.ToString().ToLower(),
                    ["runtimeVersion"] = fa.RuntimeVersion,
                    ["httpsOnly"] = fa.HttpsOnly.ToString().ToLower(),
                    ["appServicePlanId"] = fa.AppServicePlanId.Value.ToString(),
                    ["deploymentMode"] = fa.DeploymentMode.Value.ToString(),
                    ["containerRegistryId"] = fa.ContainerRegistryId?.Value.ToString() ?? "",
                    ["acrAuthMode"] = fa.AcrAuthMode?.Value.ToString() ?? "",
                    ["acrPullIdentityId"] = fa.AcrPullIdentityId?.Value.ToString() ?? "",
                    ["dockerImageName"] = fa.DockerImageName ?? "",
                    ["dockerImageValidated"] = fa.DockerImageValidated.ToString().ToLowerInvariant()
                },
                faSettings
                    .Where(es => es.FunctionAppId == fa.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            UserAssignedIdentity uai => new AzureResourceReadModel(
                uai.Id.Value,
                uai.Name.Value,
                MapLocation(uai.Location),
                AzureResourceTypes.ArmTypes.UserAssignedIdentityType,
                new Dictionary<string, string>(),
                new List<ResourceEnvironmentConfigReadModel>()),
            AppConfiguration ac => new AzureResourceReadModel(
                ac.Id.Value,
                ac.Name.Value,
                MapLocation(ac.Location),
                AzureResourceTypes.ArmTypes.AppConfigurationType,
                new Dictionary<string, string>(),
                acSettings
                    .Where(es => es.AppConfigurationId == ac.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            ContainerAppEnvironment cae => new AzureResourceReadModel(
                cae.Id.Value,
                cae.Name.Value,
                MapLocation(cae.Location),
                AzureResourceTypes.ArmTypes.ContainerAppEnvironmentType,
                cae.LogAnalyticsWorkspaceId is not null
                    ? new Dictionary<string, string> { ["logAnalyticsWorkspaceId"] = cae.LogAnalyticsWorkspaceId.Value.ToString() }
                    : new Dictionary<string, string>(),
                caeSettings
                    .Where(es => es.ContainerAppEnvironmentId == cae.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            ContainerApp ca => new AzureResourceReadModel(
                ca.Id.Value,
                ca.Name.Value,
                MapLocation(ca.Location),
                AzureResourceTypes.ArmTypes.ContainerAppType,
                new Dictionary<string, string>
                {
                    ["containerAppEnvironmentId"] = ca.ContainerAppEnvironmentId.Value.ToString(),
                    ["containerRegistryId"] = ca.ContainerRegistryId?.Value.ToString() ?? "",
                    ["acrAuthMode"] = ca.AcrAuthMode?.Value.ToString() ?? "",
                    ["acrPullIdentityId"] = ca.AcrPullIdentityId?.Value.ToString() ?? "",
                    ["dockerImageName"] = ca.DockerImageName ?? "",
                    ["dockerImageValidated"] = ca.DockerImageValidated.ToString().ToLowerInvariant()
                },
                caSettings
                    .Where(es => es.ContainerAppId == ca.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            LogAnalyticsWorkspace law => new AzureResourceReadModel(
                law.Id.Value,
                law.Name.Value,
                MapLocation(law.Location),
                AzureResourceTypes.ArmTypes.LogAnalyticsWorkspaceType,
                new Dictionary<string, string>(),
                lawSettings
                    .Where(es => es.LogAnalyticsWorkspaceId == law.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            Domain.ApplicationInsightsAggregate.ApplicationInsights ai => new AzureResourceReadModel(
                ai.Id.Value,
                ai.Name.Value,
                MapLocation(ai.Location),
                AzureResourceTypes.ArmTypes.ApplicationInsightsType,
                new Dictionary<string, string>
                {
                    ["logAnalyticsWorkspaceId"] = ai.LogAnalyticsWorkspaceId.Value.ToString()
                },
                aiSettings
                    .Where(es => es.ApplicationInsightsId == ai.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            CosmosDb cosmos => new AzureResourceReadModel(
                cosmos.Id.Value,
                cosmos.Name.Value,
                MapLocation(cosmos.Location),
                AzureResourceTypes.ArmTypes.CosmosDbType,
                new Dictionary<string, string>(),
                cosmosSettings
                    .Where(es => es.CosmosDbId == cosmos.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            SqlServer sqlServer => new AzureResourceReadModel(
                sqlServer.Id.Value,
                sqlServer.Name.Value,
                MapLocation(sqlServer.Location),
                AzureResourceTypes.ArmTypes.SqlServerType,
                new Dictionary<string, string>
                {
                    ["version"] = sqlServer.Version.Value.ToString(),
                    ["administratorLogin"] = sqlServer.AdministratorLogin
                },
                sqlServerSettings
                    .Where(es => es.SqlServerId == sqlServer.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            SqlDatabase sqlDb => new AzureResourceReadModel(
                sqlDb.Id.Value,
                sqlDb.Name.Value,
                MapLocation(sqlDb.Location),
                AzureResourceTypes.ArmTypes.SqlDatabaseType,
                new Dictionary<string, string>
                {
                    ["sqlServerId"] = sqlDb.SqlServerId.Value.ToString(),
                    ["collation"] = sqlDb.Collation
                },
                sqlDbSettings
                    .Where(es => es.SqlDatabaseId == sqlDb.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            ServiceBusNamespace sb => new AzureResourceReadModel(
                sb.Id.Value,
                sb.Name.Value,
                MapLocation(sb.Location),
                AzureResourceTypes.ArmTypes.ServiceBusNamespaceType,
                new Dictionary<string, string>(),
                sbSettings
                    .Where(es => es.ServiceBusNamespaceId == sb.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            ContainerRegistry cr => new AzureResourceReadModel(
                cr.Id.Value,
                cr.Name.Value,
                MapLocation(cr.Location),
                AzureResourceTypes.ArmTypes.ContainerRegistryType,
                new Dictionary<string, string>(),
                crSettings
                    .Where(es => es.ContainerRegistryId == cr.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            EventHubNamespace eh => new AzureResourceReadModel(
                eh.Id.Value,
                eh.Name.Value,
                MapLocation(eh.Location),
                AzureResourceTypes.ArmTypes.EventHubNamespaceType,
                new Dictionary<string, string>(),
                ehSettings
                    .Where(es => es.EventHubNamespaceId == eh.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            DocumentIntelligence di => new AzureResourceReadModel(
                di.Id.Value,
                di.Name.Value,
                MapLocation(di.Location),
                AzureResourceTypes.ArmTypes.DocumentIntelligenceType,
                new Dictionary<string, string>
                {
                    ["customSubDomainName"] = di.CustomSubDomainName ?? string.Empty,
                },
                context.DocIntSettings
                    .Where(es => es.DocumentIntelligenceId == di.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            _ => null
        };
    }

    private static string MapLocation(Location location)
    {
        return Location.ToAzureRegionKey(location);
    }

    private static List<RoleAssignmentReadModel> BuildRoleAssignmentReadModels(
        IReadOnlyCollection<RoleAssignment> roleAssignments,
        Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)> allResources,
        Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)> externalTargets)
    {
        return roleAssignments
            .Select(ra => MapRoleAssignment(ra, allResources, externalTargets))
            .OfType<RoleAssignmentReadModel>()
            .ToList();
    }

    private static RoleAssignmentReadModel? MapRoleAssignment(
        RoleAssignment ra,
        Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)> allResources,
        Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)> externalTargets)
    {
        if (!allResources.TryGetValue(ra.SourceResourceId, out var source))
            return null;

        var hasTarget = allResources.TryGetValue(ra.TargetResourceId, out var target)
            || externalTargets.TryGetValue(ra.TargetResourceId, out target);

        var (uaiName, uaiRgName) = ResolveUserAssignedIdentity(ra.UserAssignedIdentityId, allResources);

        return new RoleAssignmentReadModel(
            SourceResourceId: ra.SourceResourceId.Value,
            SourceResourceName: source.Resource.Name.Value,
            SourceResourceType: GetResourceTypeString(source.Resource),
            SourceResourceGroupName: source.ResourceGroup.Name.Value,
            TargetResourceId: ra.TargetResourceId.Value,
            TargetResourceName: hasTarget ? target.Resource.Name.Value : string.Empty,
            TargetResourceType: hasTarget ? GetResourceTypeString(target.Resource) : string.Empty,
            TargetResourceGroupName: hasTarget ? target.ResourceGroup.Name.Value : string.Empty,
            ManagedIdentityType: ra.ManagedIdentityType.Value.ToString(),
            RoleDefinitionId: ra.RoleDefinitionId,
            UserAssignedIdentityResourceId: ra.UserAssignedIdentityId?.Value,
            UserAssignedIdentityName: uaiName,
            UserAssignedIdentityResourceGroupName: uaiRgName);
    }

    private static (string? Name, string? ResourceGroupName) ResolveUserAssignedIdentity(
        AzureResourceId? userAssignedIdentityId,
        Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)> allResources)
    {
        if (userAssignedIdentityId is null)
            return (null, null);

        if (!allResources.TryGetValue(userAssignedIdentityId, out var uai))
            return (null, null);

        return (uai.Resource.Name.Value, uai.ResourceGroup.Name.Value);
    }

    private static List<AppSettingReadModel> BuildAppSettingReadModels(
        IReadOnlyCollection<AppSetting> appSettings,
        Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)> allResources,
        Dictionary<ProjectPipelineVariableGroupId, string> varGroupLookup)
    {
        return appSettings
            .Select(s => MapAppSetting(s, allResources, varGroupLookup))
            .OfType<AppSettingReadModel>()
            .ToList();
    }

    private static AppSettingReadModel? MapAppSetting(
        AppSetting s,
        Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)> allResources,
        Dictionary<ProjectPipelineVariableGroupId, string> varGroupLookup)
    {
        if (!allResources.TryGetValue(s.ResourceId, out var owner))
            return null;

        var (sourceResourceName, sourceResourceType) = ResolveSourceResourceInfo(s.SourceResourceId, allResources);
        var keyVaultResourceName = ResolveKeyVaultResourceName(s.KeyVaultResourceId, allResources);
        var variableGroupName = s.VariableGroupId is not null && varGroupLookup.TryGetValue(s.VariableGroupId, out var vgName)
            ? vgName
            : null;

        return new AppSettingReadModel(
            ResourceId: s.ResourceId.Value,
            ResourceName: owner.Resource.Name.Value,
            ResourceType: GetResourceTypeString(owner.Resource),
            Name: s.Name,
            EnvironmentValues: s.EnvironmentValues.Count > 0
                ? s.EnvironmentValues.ToDictionary(ev => ev.EnvironmentName, ev => ev.Value)
                : null,
            SourceResourceId: s.SourceResourceId?.Value,
            SourceResourceName: sourceResourceName,
            SourceResourceType: sourceResourceType,
            SourceOutputName: s.SourceOutputName,
            IsOutputReference: s.IsOutputReference,
            KeyVaultResourceId: s.KeyVaultResourceId?.Value,
            KeyVaultResourceName: keyVaultResourceName,
            SecretName: s.SecretName,
            IsKeyVaultReference: s.IsKeyVaultReference,
            SecretValueAssignment: s.SecretValueAssignment?.ToString(),
            VariableGroupId: s.VariableGroupId?.Value,
            PipelineVariableName: s.PipelineVariableName,
            VariableGroupName: variableGroupName,
            IsViaVariableGroup: s.IsViaVariableGroup);
    }

    private static (string? Name, string? Type) ResolveSourceResourceInfo(
        AzureResourceId? sourceResourceId,
        Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)> allResources)
    {
        if (sourceResourceId is null || !allResources.TryGetValue(sourceResourceId, out var sourceRes))
            return (null, null);

        return (sourceRes.Resource.Name.Value, GetResourceTypeString(sourceRes.Resource));
    }

    private static string? ResolveKeyVaultResourceName(
        AzureResourceId? keyVaultResourceId,
        Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)> allResources)
    {
        if (keyVaultResourceId is null || !allResources.TryGetValue(keyVaultResourceId, out var kvRes))
            return null;

        return kvRes.Resource.Name.Value;
    }

    private async Task<Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)>> LoadExternalTargetsAsync(
        List<AzureResourceId> externalTargetIds,
        CancellationToken cancellationToken)
    {
        var externalTargets = new Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)>();
        if (externalTargetIds.Count == 0)
            return externalTargets;

        var externalRgs = await dbContext.Set<Domain.ResourceGroupAggregate.ResourceGroup>()
            .Include(rg => rg.Resources)
            .Where(rg => rg.Resources.Any(r => externalTargetIds.Contains(r.Id)))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var rg in externalRgs)
        {
            foreach (var r in rg.Resources.Where(r => externalTargetIds.Contains(r.Id)))
            {
                externalTargets[r.Id] = (r, rg);
            }
        }

        return externalTargets;
    }

    private static List<ResourceGroupReadModel> BuildResourceGroupReadModels(
        IReadOnlyCollection<Domain.ResourceGroupAggregate.ResourceGroup> resourceGroups,
        Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)> allResources,
        IReadOnlyList<CustomDomain> customDomains,
        Func<AzureResource, AzureResourceReadModel?> mapResource)
    {
        return resourceGroups.Select(rg =>
        {
            var resources = rg.Resources
                .Select(r => MapResourceWithEnrichments(r, allResources, customDomains, mapResource))
                .OfType<AzureResourceReadModel>()
                .ToList();

            return new ResourceGroupReadModel(
                rg.Id.Value,
                rg.Name.Value,
                MapLocation(rg.Location),
                resources);
        }).ToList();
    }

    private static AzureResourceReadModel? MapResourceWithEnrichments(
        AzureResource r,
        Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)> allResources,
        IReadOnlyList<CustomDomain> customDomains,
        Func<AzureResource, AzureResourceReadModel?> mapResource)
    {
        var readModel = mapResource(r);
        if (readModel is null) return null;

        string? assignedUaiName = null;
        if (r.AssignedUserAssignedIdentityId is not null
            && allResources.TryGetValue(r.AssignedUserAssignedIdentityId, out var uaiEntry))
        {
            assignedUaiName = uaiEntry.Resource.Name.Value;
        }

        var resourceCustomDomains = customDomains
            .Where(cd => cd.ResourceId == r.Id)
            .Select(cd => new CustomDomainReadModel(cd.EnvironmentName, cd.DomainName, cd.CertificateMode.Value.ToString(), cd.KeyVaultUrl, cd.ManagedIdentityResourceId, cd.CertificateName, cd.DnsValidationStatus.Value.ToString()))
            .ToList();

        return readModel with
        {
            AssignedUserAssignedIdentityName = assignedUaiName,
            IsExisting = r.IsExisting,
            CustomDomains = resourceCustomDomains
        };
    }

    private async Task<List<CrossConfigReferenceReadModel>> BuildCrossConfigReferenceReadModelsAsync(
        IReadOnlyCollection<CrossConfigResourceReference> crossConfigReferences,
        NamingContextReadModel namingContext,
        CancellationToken cancellationToken)
    {
        var result = new List<CrossConfigReferenceReadModel>();
        foreach (var ccRef in crossConfigReferences)
        {
            var readModel = await MapCrossConfigReferenceAsync(ccRef, namingContext, cancellationToken);
            if (readModel is not null)
                result.Add(readModel);
        }
        return result;
    }

    private async Task<CrossConfigReferenceReadModel?> MapCrossConfigReferenceAsync(
        CrossConfigResourceReference ccRef,
        NamingContextReadModel namingContext,
        CancellationToken cancellationToken)
    {
        var targetConfig = await dbContext.InfrastructureConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == ccRef.TargetConfigId, cancellationToken);

        var targetRg = await dbContext.Set<Domain.ResourceGroupAggregate.ResourceGroup>()
            .Include(rg => rg.Resources)
            .AsNoTracking()
            .FirstOrDefaultAsync(rg => rg.Resources.Any(r => r.Id == ccRef.TargetResourceId), cancellationToken);

        if (targetConfig is null || targetRg is null) return null;

        var targetResource = targetRg.Resources.FirstOrDefault(r => r.Id == ccRef.TargetResourceId);
        if (targetResource is null) return null;

        var resourceTypeName = GetResourceTypeString(targetResource);
        var simpleTypeName = GetResourceTypeName(targetResource);
        var abbreviation = namingContext.ResourceAbbreviations.TryGetValue(simpleTypeName, out var overrideAbbr)
            ? overrideAbbr
            : ResourceAbbreviationCatalog.GetAbbreviation(simpleTypeName);

        return new CrossConfigReferenceReadModel(
            ReferenceId: ccRef.Id.Value,
            TargetConfigId: ccRef.TargetConfigId.Value,
            TargetConfigName: targetConfig.Name.Value,
            TargetResourceId: ccRef.TargetResourceId.Value,
            TargetResourceName: targetResource.Name.Value,
            TargetResourceType: resourceTypeName,
            TargetResourceGroupName: targetRg.Name.Value,
            TargetResourceAbbreviation: abbreviation);
    }

    private static List<RoleAssignmentReadModel> EnrichRoleAssignmentsWithCrossConfig(
        IReadOnlyCollection<RoleAssignmentReadModel> roleAssignmentReadModels,
        IReadOnlyCollection<CrossConfigResourceReference> crossConfigReferences,
        IReadOnlyDictionary<Guid, CrossConfigReferenceReadModel> crossConfigRefLookup)
    {
        var crossConfigResourceIds = crossConfigReferences
            .Select(r => r.TargetResourceId)
            .ToHashSet();

        return roleAssignmentReadModels
            .Select(ra => EnrichSingleRoleAssignment(ra, crossConfigResourceIds, crossConfigRefLookup))
            .ToList();
    }

    private static RoleAssignmentReadModel EnrichSingleRoleAssignment(
        RoleAssignmentReadModel ra,
        HashSet<AzureResourceId> crossConfigResourceIds,
        IReadOnlyDictionary<Guid, CrossConfigReferenceReadModel> crossConfigRefLookup)
    {
        var isCrossConfig = crossConfigResourceIds.Contains(new AzureResourceId(ra.TargetResourceId));
        if (isCrossConfig && crossConfigRefLookup.TryGetValue(ra.TargetResourceId, out var ccRef))
        {
            return ra with
            {
                IsTargetCrossConfig = true,
                TargetResourceName = string.IsNullOrEmpty(ra.TargetResourceName) ? ccRef.TargetResourceName : ra.TargetResourceName,
                TargetResourceType = string.IsNullOrEmpty(ra.TargetResourceType) ? ccRef.TargetResourceType : ra.TargetResourceType,
                TargetResourceGroupName = string.IsNullOrEmpty(ra.TargetResourceGroupName) ? ccRef.TargetResourceGroupName : ra.TargetResourceGroupName,
            };
        }
        return ra with { IsTargetCrossConfig = isCrossConfig };
    }

    private static List<AppSettingReadModel> EnrichAppSettingsWithCrossConfig(
        IReadOnlyCollection<AppSettingReadModel> appSettingReadModels,
        IReadOnlyDictionary<Guid, CrossConfigReferenceReadModel> crossConfigRefLookup)
    {
        return appSettingReadModels
            .Select(s =>
            {
                if (s.SourceResourceId is not null && crossConfigRefLookup.TryGetValue(s.SourceResourceId.Value, out var ccSrc))
                    return s with { IsSourceCrossConfig = true, SourceResourceGroupName = ccSrc.TargetResourceGroupName };
                return s;
            })
            .ToList();
    }

    private static List<SecureParameterMappingReadModel> BuildSecureParameterReadModels(
        IReadOnlyCollection<SecureParameterMapping> secureParameterMappings,
        Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)> allResources,
        Dictionary<ProjectPipelineVariableGroupId, string> varGroupLookup)
    {
        return secureParameterMappings
            .Select(m => MapSecureParameter(m, allResources, varGroupLookup))
            .OfType<SecureParameterMappingReadModel>()
            .ToList();
    }

    private static SecureParameterMappingReadModel? MapSecureParameter(
        SecureParameterMapping m,
        Dictionary<AzureResourceId, (AzureResource Resource, Domain.ResourceGroupAggregate.ResourceGroup ResourceGroup)> allResources,
        Dictionary<ProjectPipelineVariableGroupId, string> varGroupLookup)
    {
        if (!allResources.TryGetValue(m.ResourceId, out var owner))
            return null;

        string? vgName = m.VariableGroupId is not null
            && varGroupLookup.TryGetValue(m.VariableGroupId, out var name)
                ? name
                : null;

        return new SecureParameterMappingReadModel(
            Id: m.Id.Value,
            ResourceId: m.ResourceId.Value,
            ResourceName: owner.Resource.Name.Value,
            SecureParameterName: m.SecureParameterName,
            VariableGroupId: m.VariableGroupId?.Value,
            VariableGroupName: vgName,
            PipelineVariableName: m.PipelineVariableName);
    }

    /// <summary>
    /// Maps an <see cref="AzureResource"/> to its Azure resource type string.
    /// </summary>
    public static string GetResourceTypeString(AzureResource resource) =>
        resource switch
        {
            KeyVault => AzureResourceTypes.ArmTypes.KeyVaultType,
            RedisCache => AzureResourceTypes.ArmTypes.RedisCacheType,
            StorageAccount => AzureResourceTypes.ArmTypes.StorageAccountType,
            AppServicePlan => AzureResourceTypes.ArmTypes.AppServicePlanType,
            WebApp => AzureResourceTypes.ArmTypes.WebAppType,
            FunctionApp => AzureResourceTypes.ArmTypes.FunctionAppType,
            UserAssignedIdentity => AzureResourceTypes.ArmTypes.UserAssignedIdentityType,
            AppConfiguration => AzureResourceTypes.ArmTypes.AppConfigurationType,
            ContainerAppEnvironment => AzureResourceTypes.ArmTypes.ContainerAppEnvironmentType,
            ContainerApp => AzureResourceTypes.ArmTypes.ContainerAppType,
            LogAnalyticsWorkspace => AzureResourceTypes.ArmTypes.LogAnalyticsWorkspaceType,
            Domain.ApplicationInsightsAggregate.ApplicationInsights => AzureResourceTypes.ArmTypes.ApplicationInsightsType,
            CosmosDb => AzureResourceTypes.ArmTypes.CosmosDbType,
            SqlServer => AzureResourceTypes.ArmTypes.SqlServerType,
            SqlDatabase => AzureResourceTypes.ArmTypes.SqlDatabaseType,
            ServiceBusNamespace => AzureResourceTypes.ArmTypes.ServiceBusNamespaceType,
            ContainerRegistry => AzureResourceTypes.ArmTypes.ContainerRegistryType,
            EventHubNamespace => AzureResourceTypes.ArmTypes.EventHubNamespaceType,
            DocumentIntelligence => AzureResourceTypes.ArmTypes.DocumentIntelligenceType,
            _ => resource.GetType().Name
        };

    /// <summary>
    /// Maps an <see cref="AzureResource"/> to its simple type name (e.g. "KeyVault", "StorageAccount").
    /// </summary>
    public static string GetResourceTypeName(AzureResource resource) =>
        resource switch
        {
            KeyVault => AzureResourceTypes.KeyVault,
            RedisCache => AzureResourceTypes.RedisCache,
            StorageAccount => AzureResourceTypes.StorageAccount,
            AppServicePlan => AzureResourceTypes.AppServicePlan,
            WebApp => AzureResourceTypes.WebApp,
            FunctionApp => AzureResourceTypes.FunctionApp,
            UserAssignedIdentity => AzureResourceTypes.UserAssignedIdentity,
            AppConfiguration => AzureResourceTypes.AppConfiguration,
            ContainerAppEnvironment => AzureResourceTypes.ContainerAppEnvironment,
            ContainerApp => AzureResourceTypes.ContainerApp,
            LogAnalyticsWorkspace => AzureResourceTypes.LogAnalyticsWorkspace,
            Domain.ApplicationInsightsAggregate.ApplicationInsights => AzureResourceTypes.ApplicationInsights,
            CosmosDb => AzureResourceTypes.CosmosDb,
            SqlServer => AzureResourceTypes.SqlServer,
            SqlDatabase => AzureResourceTypes.SqlDatabase,
            ServiceBusNamespace => AzureResourceTypes.ServiceBusNamespace,
            EventHubNamespace => AzureResourceTypes.EventHubNamespace,
            DocumentIntelligence => AzureResourceTypes.DocumentIntelligence,
            _ => resource.GetType().Name
        };
}

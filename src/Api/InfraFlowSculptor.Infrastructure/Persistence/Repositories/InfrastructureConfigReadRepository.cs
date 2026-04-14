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
using InfraFlowSculptor.Domain.ApplicationInsightsAggregate;
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
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.Infrastructure.Persistence;
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

        // ── Load role assignments for all resources in this config ───────────
        var roleAssignments = await dbContext.RoleAssignments
            .Where(ra => allResourceIds.Contains(ra.SourceResourceId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // ── Load app settings for all resources in this config ──────────────
        var appSettings = await dbContext.AppSettings
            .Include(s => s.EnvironmentValues)
            .Where(s => allResourceIds.Contains(s.ResourceId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Build a flat lookup of all resources by ID across all resource groups
        var allResources = config.ResourceGroups
            .SelectMany(rg => rg.Resources.Select(r => (Resource: r, ResourceGroup: rg)))
            .ToDictionary(x => x.Resource.Id);

        var roleAssignmentReadModels = roleAssignments
            .Select(ra =>
            {
                if (!allResources.TryGetValue(ra.SourceResourceId, out var source))
                    return null;

                // Target resource might be in a different resource group, look it up too
                var hasTarget = allResources.TryGetValue(ra.TargetResourceId, out var target);

                string? uaiName = null;
                string? uaiRgName = null;
                if (ra.UserAssignedIdentityId is not null &&
                    allResources.TryGetValue(ra.UserAssignedIdentityId, out var uai))
                {
                    uaiName = uai.Resource.Name.Value;
                    uaiRgName = uai.ResourceGroup.Name.Value;
                }

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
            })
            .OfType<RoleAssignmentReadModel>()
            .ToList();

        var resourceGroups = config.ResourceGroups.Select(rg =>
        {
            var resources = rg.Resources
                .Select(r =>
                {
                    var readModel = MapResource(r, kvSettings, rcSettings, saSettings, blobContainers, storageQueues, storageTables, storageCorsRules, lifecycleRules, aspSettings, waSettings, faSettings, acSettings, caeSettings, caSettings, lawSettings, aiSettings, cosmosSettings, sqlServerSettings, sqlDbSettings, sbSettings, crSettings, ehSettings);
                    if (readModel is null) return null;

                    // Resolve the assigned UAI name from the resource's FK
                    string? assignedUaiName = null;
                    if (r.AssignedUserAssignedIdentityId is not null
                        && allResources.TryGetValue(r.AssignedUserAssignedIdentityId, out var uaiEntry))
                    {
                        assignedUaiName = uaiEntry.Resource.Name.Value;
                    }

                    return readModel with { AssignedUserAssignedIdentityName = assignedUaiName };
                })
                .OfType<AzureResourceReadModel>()
                .ToList();

            return new ResourceGroupReadModel(
                rg.Id.Value,
                rg.Name.Value,
                MapLocation(rg.Location),
                resources);
        }).ToList();

        // ── Load parent project for environments and naming context ─────────
        var project = await dbContext.Projects
            .Include(p => p.ResourceNamingTemplates)
            .Include(p => p.Tags)
            .Include(p => p.EnvironmentDefinitions)
                .ThenInclude(e => e.Tags)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == config.ProjectId, cancellationToken);

        var environments = BuildEnvironmentList(config, project);
        var namingContext = BuildNamingContext(config, project);

        // ── Load pipeline variable groups for VG name resolution ────────────
        var projectVarGroups = project is not null
            ? await dbContext.ProjectPipelineVariableGroups
                .Where(g => g.ProjectId == project.Id)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
            : [];

        var varGroupLookup = projectVarGroups.ToDictionary(g => g.Id, g => g.GroupName);

        var appSettingReadModels = appSettings
            .Select(s =>
            {
                if (!allResources.TryGetValue(s.ResourceId, out var owner))
                    return null;

                string? sourceResourceName = null;
                string? sourceResourceType = null;
                if (s.SourceResourceId is not null &&
                    allResources.TryGetValue(s.SourceResourceId, out var sourceRes))
                {
                    sourceResourceName = sourceRes.Resource.Name.Value;
                    sourceResourceType = GetResourceTypeString(sourceRes.Resource);
                }

                string? keyVaultResourceName = null;
                if (s.KeyVaultResourceId is not null &&
                    allResources.TryGetValue(s.KeyVaultResourceId, out var kvRes))
                {
                    keyVaultResourceName = kvRes.Resource.Name.Value;
                }

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
                    VariableGroupName: s.VariableGroupId is not null && varGroupLookup.TryGetValue(s.VariableGroupId, out var vgName) ? vgName : null,
                    IsViaVariableGroup: s.IsViaVariableGroup);
            })
            .OfType<AppSettingReadModel>()
            .ToList();

        // ── Load cross-config resource references ───────────────────────────
        var crossConfigReferences = await dbContext.CrossConfigResourceReferences
            .Where(r => r.InfraConfigId == configId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var crossConfigRefReadModels = new List<CrossConfigReferenceReadModel>();
        foreach (var ccRef in crossConfigReferences)
        {
            var targetConfig = await dbContext.InfrastructureConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == ccRef.TargetConfigId, cancellationToken);

            var targetRg = await dbContext.Set<Domain.ResourceGroupAggregate.ResourceGroup>()
                .Include(rg => rg.Resources)
                .AsNoTracking()
                .FirstOrDefaultAsync(rg => rg.Resources.Any(r => r.Id == ccRef.TargetResourceId), cancellationToken);

            if (targetConfig is null || targetRg is null) continue;

            var targetResource = targetRg.Resources.FirstOrDefault(r => r.Id == ccRef.TargetResourceId);
            if (targetResource is null) continue;

            var resourceTypeName = GetResourceTypeString(targetResource);
            var simpleTypeName = GetResourceTypeName(targetResource);
            var abbreviation = ResourceAbbreviationCatalog.GetAbbreviation(simpleTypeName);

            crossConfigRefReadModels.Add(new CrossConfigReferenceReadModel(
                ReferenceId: ccRef.Id.Value,
                TargetConfigId: ccRef.TargetConfigId.Value,
                TargetConfigName: targetConfig.Name.Value,
                TargetResourceId: ccRef.TargetResourceId.Value,
                TargetResourceName: targetResource.Name.Value,
                TargetResourceType: resourceTypeName,
                TargetResourceGroupName: targetRg.Name.Value,
                TargetResourceAbbreviation: abbreviation));
        }

        // ── Enrich role assignments with cross-config info ──────────────────
        var crossConfigResourceIds = crossConfigReferences
            .Select(r => r.TargetResourceId)
            .ToHashSet();

        var enrichedRoleAssignments = roleAssignmentReadModels
            .Select(ra => ra with { IsTargetCrossConfig = crossConfigResourceIds.Contains(new AzureResourceId(ra.TargetResourceId)) })
            .ToList();

        // ── Enrich app settings with cross-config info ──────────────────────
        var crossConfigRefLookup = crossConfigRefReadModels
            .ToDictionary(r => r.TargetResourceId);

        var enrichedAppSettings = appSettingReadModels
            .Select(s =>
            {
                if (s.SourceResourceId is not null && crossConfigRefLookup.TryGetValue(s.SourceResourceId.Value, out var ccSrc))
                    return s with { IsSourceCrossConfig = true, SourceResourceGroupName = ccSrc.TargetResourceGroupName };
                return s;
            })
            .ToList();

        var projectTags = project?.Tags
            .ToDictionary(t => t.Name, t => t.Value)
            ?? new Dictionary<string, string>();

        var configTags = config.Tags
            .ToDictionary(t => t.Name, t => t.Value);

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
            config.AppPipelineMode.ToString());
    }

    /// <summary>
    /// Resolves the environment list from the parent project.
    /// </summary>
    private static List<EnvironmentDefinitionReadModel> BuildEnvironmentList(
        InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig config,
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
                e.SubscriptionId.Value.ToString(),
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
                    t => t.Template.Value));
        }

        // Otherwise, read from the config itself (if overridden)
        return new NamingContextReadModel(
            config.DefaultNamingTemplate?.Value,
            config.ResourceNamingTemplates.ToDictionary(
                t => t.ResourceType,
                t => t.Template.Value));
    }

    /// <summary>
    /// Maps an <see cref="AzureResource"/> to its read model using typed environment settings.
    /// Returns <c>null</c> for resource types that are not yet supported by the generator.
    /// </summary>
    private static AzureResourceReadModel? MapResource(
        AzureResource resource,
        IReadOnlyList<KeyVaultEnvironmentSettings> kvSettings,
        IReadOnlyList<RedisCacheEnvironmentSettings> rcSettings,
        IReadOnlyList<StorageAccountEnvironmentSettings> saSettings,
        IReadOnlyList<BlobContainer> blobContainers,
        IReadOnlyList<StorageQueue> storageQueues,
        IReadOnlyList<StorageTable> storageTables,
        IReadOnlyList<CorsRule> storageCorsRules,
        IReadOnlyList<BlobLifecycleRule> lifecycleRules,
        IReadOnlyList<AppServicePlanEnvironmentSettings> aspSettings,
        IReadOnlyList<WebAppEnvironmentSettings> waSettings,
        IReadOnlyList<FunctionAppEnvironmentSettings> faSettings,
        IReadOnlyList<AppConfigurationEnvironmentSettings> acSettings,
        IReadOnlyList<ContainerAppEnvironmentEnvironmentSettings> caeSettings,
        IReadOnlyList<ContainerAppEnvironmentSettings> caSettings,
        IReadOnlyList<LogAnalyticsWorkspaceEnvironmentSettings> lawSettings,
        IReadOnlyList<ApplicationInsightsEnvironmentSettings> aiSettings,
        IReadOnlyList<CosmosDbEnvironmentSettings> cosmosSettings,
        IReadOnlyList<SqlServerEnvironmentSettings> sqlServerSettings,
        IReadOnlyList<SqlDatabaseEnvironmentSettings> sqlDbSettings,
        IReadOnlyList<ServiceBusNamespaceEnvironmentSettings> sbSettings,
        IReadOnlyList<ContainerRegistryEnvironmentSettings> crSettings,
        IReadOnlyList<EventHubNamespaceEnvironmentSettings> ehSettings)
    {
        return resource switch
        {
            KeyVault kv => new AzureResourceReadModel(
                kv.Id.Value,
                kv.Name.Value,
                MapLocation(kv.Location),
                "Microsoft.KeyVault/vaults",
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
                "Microsoft.Cache/Redis",
                new Dictionary<string, string>(),
                rcSettings
                    .Where(es => es.RedisCacheId == rc.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            StorageAccount sa => new AzureResourceReadModel(
                sa.Id.Value,
                sa.Name.Value,
                MapLocation(sa.Location),
                "Microsoft.Storage/storageAccounts",
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
                "Microsoft.Web/serverfarms",
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
                "Microsoft.Web/sites",
                new Dictionary<string, string>
                {
                    ["runtimeStack"] = wa.RuntimeStack.Value.ToString().ToLower(),
                    ["runtimeVersion"] = wa.RuntimeVersion,
                    ["alwaysOn"] = wa.AlwaysOn.ToString().ToLower(),
                    ["httpsOnly"] = wa.HttpsOnly.ToString().ToLower(),
                    ["appServicePlanId"] = wa.AppServicePlanId.Value.ToString(),
                    ["deploymentMode"] = wa.DeploymentMode.Value.ToString(),
                    ["containerRegistryId"] = wa.ContainerRegistryId?.Value.ToString() ?? "",
                    ["dockerImageName"] = wa.DockerImageName ?? ""
                },
                waSettings
                    .Where(es => es.WebAppId == wa.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            FunctionApp fa => new AzureResourceReadModel(
                fa.Id.Value,
                fa.Name.Value,
                MapLocation(fa.Location),
                "Microsoft.Web/sites/functionapp",
                new Dictionary<string, string>
                {
                    ["runtimeStack"] = fa.RuntimeStack.Value.ToString().ToLower(),
                    ["runtimeVersion"] = fa.RuntimeVersion,
                    ["httpsOnly"] = fa.HttpsOnly.ToString().ToLower(),
                    ["appServicePlanId"] = fa.AppServicePlanId.Value.ToString(),
                    ["deploymentMode"] = fa.DeploymentMode.Value.ToString(),
                    ["containerRegistryId"] = fa.ContainerRegistryId?.Value.ToString() ?? "",
                    ["dockerImageName"] = fa.DockerImageName ?? ""
                },
                faSettings
                    .Where(es => es.FunctionAppId == fa.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            UserAssignedIdentity uai => new AzureResourceReadModel(
                uai.Id.Value,
                uai.Name.Value,
                MapLocation(uai.Location),
                "Microsoft.ManagedIdentity/userAssignedIdentities",
                new Dictionary<string, string>(),
                new List<ResourceEnvironmentConfigReadModel>()),
            AppConfiguration ac => new AzureResourceReadModel(
                ac.Id.Value,
                ac.Name.Value,
                MapLocation(ac.Location),
                "Microsoft.AppConfiguration/configurationStores",
                new Dictionary<string, string>(),
                acSettings
                    .Where(es => es.AppConfigurationId == ac.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            ContainerAppEnvironment cae => new AzureResourceReadModel(
                cae.Id.Value,
                cae.Name.Value,
                MapLocation(cae.Location),
                "Microsoft.App/managedEnvironments",
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
                "Microsoft.App/containerApps",
                new Dictionary<string, string>
                {
                    ["containerAppEnvironmentId"] = ca.ContainerAppEnvironmentId.Value.ToString(),
                    ["containerRegistryId"] = ca.ContainerRegistryId?.Value.ToString() ?? "",
                    ["dockerImageName"] = ca.DockerImageName ?? ""
                },
                caSettings
                    .Where(es => es.ContainerAppId == ca.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            LogAnalyticsWorkspace law => new AzureResourceReadModel(
                law.Id.Value,
                law.Name.Value,
                MapLocation(law.Location),
                "Microsoft.OperationalInsights/workspaces",
                new Dictionary<string, string>(),
                lawSettings
                    .Where(es => es.LogAnalyticsWorkspaceId == law.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            Domain.ApplicationInsightsAggregate.ApplicationInsights ai => new AzureResourceReadModel(
                ai.Id.Value,
                ai.Name.Value,
                MapLocation(ai.Location),
                "Microsoft.Insights/components",
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
                "Microsoft.DocumentDB/databaseAccounts",
                new Dictionary<string, string>(),
                cosmosSettings
                    .Where(es => es.CosmosDbId == cosmos.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            SqlServer sqlServer => new AzureResourceReadModel(
                sqlServer.Id.Value,
                sqlServer.Name.Value,
                MapLocation(sqlServer.Location),
                "Microsoft.Sql/servers",
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
                "Microsoft.Sql/servers/databases",
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
                "Microsoft.ServiceBus/namespaces",
                new Dictionary<string, string>(),
                sbSettings
                    .Where(es => es.ServiceBusNamespaceId == sb.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            ContainerRegistry cr => new AzureResourceReadModel(
                cr.Id.Value,
                cr.Name.Value,
                MapLocation(cr.Location),
                "Microsoft.ContainerRegistry/registries",
                new Dictionary<string, string>(),
                crSettings
                    .Where(es => es.ContainerRegistryId == cr.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            EventHubNamespace eh => new AzureResourceReadModel(
                eh.Id.Value,
                eh.Name.Value,
                MapLocation(eh.Location),
                "Microsoft.EventHub/namespaces",
                new Dictionary<string, string>(),
                ehSettings
                    .Where(es => es.EventHubNamespaceId == eh.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            _ => null
        };
    }

    private static string MapLocation(Location location)
    {
        return location.Value switch
        {
            Location.LocationEnum.FranceCentral => "francecentral",
            Location.LocationEnum.FranceSouth => "francesouth",
            Location.LocationEnum.UKSouth => "uksouth",
            Location.LocationEnum.WestEurope => "westeurope",
            Location.LocationEnum.GermanyWestCentral => "germanywestcentral",
            Location.LocationEnum.SwitzerlandNorth => "switzerlandnorth",
            Location.LocationEnum.ItalyNorth => "italynorth",
            Location.LocationEnum.NorthEurope => "northeurope",
            Location.LocationEnum.SpainCentral => "spaincentral",
            Location.LocationEnum.NorwayEast => "norwayeast",
            Location.LocationEnum.PolandCentral => "polandcentral",
            Location.LocationEnum.SwedenCentral => "swedencentral",
            Location.LocationEnum.QatarCentral => "qatarcentral",
            Location.LocationEnum.UAENorth => "uaenorth",
            Location.LocationEnum.CanadaEast => "canadaeast",
            Location.LocationEnum.CanadaCentral => "canadacentral",
            Location.LocationEnum.EastUS => "eastus",
            Location.LocationEnum.EastUS2 => "eastus2",
            Location.LocationEnum.CentralIndia => "centralindia",
            Location.LocationEnum.CentralUS => "centralus",
            Location.LocationEnum.SouthCentralUS => "southcentralus",
            Location.LocationEnum.WestUS2 => "westus2",
            Location.LocationEnum.SouthAfricaNorth => "southafricanorth",
            Location.LocationEnum.WestUS3 => "westus3",
            Location.LocationEnum.WestUS => "westus",
            Location.LocationEnum.KoreaCentral => "koreacentral",
            Location.LocationEnum.BrazilSouth => "brazilsouth",
            Location.LocationEnum.EastAsia => "eastasia",
            Location.LocationEnum.JapanEast => "japaneast",
            Location.LocationEnum.SoutheastAsia => "southeastasia",
            Location.LocationEnum.AustraliaEast => "australiaeast",
            _ => "westeurope"
        };
    }

    /// <summary>
    /// Maps an <see cref="AzureResource"/> to its Azure resource type string.
    /// </summary>
    public static string GetResourceTypeString(AzureResource resource) =>
        resource switch
        {
            KeyVault => AzureResourceTypes.ArmTypes.KeyVault,
            RedisCache => AzureResourceTypes.ArmTypes.RedisCache,
            StorageAccount => AzureResourceTypes.ArmTypes.StorageAccount,
            AppServicePlan => AzureResourceTypes.ArmTypes.AppServicePlan,
            WebApp => AzureResourceTypes.ArmTypes.WebApp,
            FunctionApp => AzureResourceTypes.ArmTypes.FunctionApp,
            UserAssignedIdentity => AzureResourceTypes.ArmTypes.UserAssignedIdentity,
            AppConfiguration => AzureResourceTypes.ArmTypes.AppConfiguration,
            ContainerAppEnvironment => AzureResourceTypes.ArmTypes.ContainerAppEnvironment,
            ContainerApp => AzureResourceTypes.ArmTypes.ContainerApp,
            LogAnalyticsWorkspace => AzureResourceTypes.ArmTypes.LogAnalyticsWorkspace,
            Domain.ApplicationInsightsAggregate.ApplicationInsights => AzureResourceTypes.ArmTypes.ApplicationInsights,
            CosmosDb => AzureResourceTypes.ArmTypes.CosmosDb,
            SqlServer => AzureResourceTypes.ArmTypes.SqlServer,
            SqlDatabase => AzureResourceTypes.ArmTypes.SqlDatabase,
            ServiceBusNamespace => AzureResourceTypes.ArmTypes.ServiceBusNamespace,
            ContainerRegistry => AzureResourceTypes.ArmTypes.ContainerRegistry,
            EventHubNamespace => AzureResourceTypes.ArmTypes.EventHubNamespace,
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
            _ => resource.GetType().Name
        };
}

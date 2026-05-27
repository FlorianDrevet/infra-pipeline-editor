using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.ApplicationInsightsAggregate;
using InfraFlowSculptor.Domain.ApplicationInsightsAggregate.Entities;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate.Entities;
using InfraFlowSculptor.Domain.CosmosDbAggregate.Entities;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.Entities;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate.Entities;
using InfraFlowSculptor.Domain.KeyVaultAggregate.Entities;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate.Entities;
using InfraFlowSculptor.Domain.RedisCacheAggregate.Entities;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.Entities;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.Entities;
using InfraFlowSculptor.Domain.SqlServerAggregate.Entities;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.Entities;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class ResourceGroupRepository : BaseRepository<ResourceGroup, ProjectDbContext>, IResourceGroupRepository
{
    public ResourceGroupRepository(ProjectDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<ResourceGroup?> GetByIdReadOnlyAsync(
        ResourceGroupId id,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<ResourceGroup>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public override async Task<ResourceGroup?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ResourceGroup>()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<ResourceGroup>> GetByInfraConfigIdAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<ResourceGroup>()
            .Include(r => r.Resources)
            .Where(r => r.InfraConfigId == infraConfigId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<ResourceGroup>> GetLightweightByInfraConfigIdAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<ResourceGroup>()
            .Where(r => r.InfraConfigId == infraConfigId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<ResourceGroup?> GetByIdWithResourcesAsync(ResourceGroupId id, CancellationToken ct = default)
    {
        return await Context.Set<ResourceGroup>()
            .Include(r => r.Resources)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<Dictionary<Guid, (int ResourceGroupCount, int ResourceCount)>> GetResourceCountsByInfraConfigIdsAsync(
        IReadOnlyList<InfrastructureConfigId> infraConfigIds,
        CancellationToken cancellationToken = default)
    {
        if (infraConfigIds.Count == 0)
            return new Dictionary<Guid, (int, int)>();

        var configIdsList = infraConfigIds.ToList();

        var counts = await Context.Set<ResourceGroup>()
            .Where(rg => configIdsList.Contains(rg.InfraConfigId))
            .Select(rg => new
            {
                rg.InfraConfigId,
                ResourceCount = rg.Resources.Count
            })
            .GroupBy(x => x.InfraConfigId)
            .Select(g => new
            {
                InfraConfigId = g.Key,
                ResourceGroupCount = g.Count(),
                ResourceCount = g.Sum(x => x.ResourceCount)
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(
            x => x.InfraConfigId.Value,
            x => (x.ResourceGroupCount, x.ResourceCount));
    }

    /// <inheritdoc />
    public async Task<Dictionary<Guid, Guid>> GetChildToParentMappingAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        var webApps = await Context.Set<WebApp>()
            .Where(x => x.ResourceGroupId == resourceGroupId)
            .Select(x => new { ChildResourceId = x.Id.Value, ParentResourceId = x.AppServicePlanId.Value })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var functionApps = await Context.Set<FunctionApp>()
            .Where(x => x.ResourceGroupId == resourceGroupId)
            .Select(x => new { ChildResourceId = x.Id.Value, ParentResourceId = x.AppServicePlanId.Value })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var containerApps = await Context.Set<ContainerApp>()
            .Where(x => x.ResourceGroupId == resourceGroupId)
            .Select(x => new { ChildResourceId = x.Id.Value, ParentResourceId = x.ContainerAppEnvironmentId.Value })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var sqlDatabases = await Context.Set<SqlDatabase>()
            .Where(x => x.ResourceGroupId == resourceGroupId)
            .Select(x => new { ChildResourceId = x.Id.Value, ParentResourceId = x.SqlServerId.Value })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var appInsights = await Context.Set<ApplicationInsights>()
            .Where(x => x.ResourceGroupId == resourceGroupId)
            .Select(x => new { ChildResourceId = x.Id.Value, ParentResourceId = x.LogAnalyticsWorkspaceId.Value })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = new Dictionary<Guid, Guid>();
        foreach (var link in webApps.Concat(functionApps).Concat(containerApps).Concat(sqlDatabases).Concat(appInsights))
        {
            result[link.ChildResourceId] = link.ParentResourceId;
        }

        return result;
    }

    public async Task<ResourceGroup?> GetByContainedResourceIdAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<ResourceGroup>()
            .Include(rg => rg.Resources)
            .FirstOrDefaultAsync(rg => rg.Resources.Any(r => r.Id == resourceId), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Dictionary<Guid, List<string>>> GetConfiguredEnvironmentsByResourceGroupAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        var resourcesInGroup = Context.AzureResources
            .Where(r => r.ResourceGroupId == resourceGroupId);

        var keyVault = await Context.Set<KeyVaultEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.KeyVaultId, r => r.Id,
                (es, _) => new { ResourceId = es.KeyVaultId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var redis = await Context.Set<RedisCacheEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.RedisCacheId, r => r.Id,
                (es, _) => new { ResourceId = es.RedisCacheId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var storage = await Context.Set<StorageAccountEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.StorageAccountId, r => r.Id,
                (es, _) => new { ResourceId = es.StorageAccountId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var appServicePlan = await Context.Set<AppServicePlanEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.AppServicePlanId, r => r.Id,
                (es, _) => new { ResourceId = es.AppServicePlanId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var webApp = await Context.Set<WebAppEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.WebAppId, r => r.Id,
                (es, _) => new { ResourceId = es.WebAppId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var functionApp = await Context.Set<FunctionAppEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.FunctionAppId, r => r.Id,
                (es, _) => new { ResourceId = es.FunctionAppId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var appConfig = await Context.Set<AppConfigurationEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.AppConfigurationId, r => r.Id,
                (es, _) => new { ResourceId = es.AppConfigurationId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var containerAppEnv = await Context.Set<ContainerAppEnvironmentEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.ContainerAppEnvironmentId, r => r.Id,
                (es, _) => new { ResourceId = es.ContainerAppEnvironmentId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var containerApp = await Context.Set<ContainerAppEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.ContainerAppId, r => r.Id,
                (es, _) => new { ResourceId = es.ContainerAppId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var logAnalytics = await Context.Set<LogAnalyticsWorkspaceEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.LogAnalyticsWorkspaceId, r => r.Id,
                (es, _) => new { ResourceId = es.LogAnalyticsWorkspaceId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var appInsights = await Context.Set<ApplicationInsightsEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.ApplicationInsightsId, r => r.Id,
                (es, _) => new { ResourceId = es.ApplicationInsightsId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var cosmosDb = await Context.Set<CosmosDbEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.CosmosDbId, r => r.Id,
                (es, _) => new { ResourceId = es.CosmosDbId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var sqlServer = await Context.Set<SqlServerEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.SqlServerId, r => r.Id,
                (es, _) => new { ResourceId = es.SqlServerId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var sqlDatabase = await Context.Set<SqlDatabaseEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.SqlDatabaseId, r => r.Id,
                (es, _) => new { ResourceId = es.SqlDatabaseId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var serviceBus = await Context.Set<ServiceBusNamespaceEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.ServiceBusNamespaceId, r => r.Id,
                (es, _) => new { ResourceId = es.ServiceBusNamespaceId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var containerRegistry = await Context.Set<ContainerRegistryEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.ContainerRegistryId, r => r.Id,
                (es, _) => new { ResourceId = es.ContainerRegistryId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var eventHub = await Context.Set<EventHubNamespaceEnvironmentSettings>()
            .Join(resourcesInGroup, es => es.EventHubNamespaceId, r => r.Id,
                (es, _) => new { ResourceId = es.EventHubNamespaceId.Value, es.EnvironmentName })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var entries = keyVault
            .Concat(redis).Concat(storage).Concat(appServicePlan)
            .Concat(webApp).Concat(functionApp).Concat(appConfig)
            .Concat(containerAppEnv).Concat(containerApp)
            .Concat(logAnalytics).Concat(appInsights).Concat(cosmosDb)
            .Concat(sqlServer).Concat(sqlDatabase).Concat(serviceBus)
            .Concat(containerRegistry).Concat(eventHub)
            .ToList();

        return entries
            .GroupBy(e => e.ResourceId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.EnvironmentName).Distinct().ToList());
    }

    /// <inheritdoc />
    public async Task<List<ResourceSummary>> GetResourceSummariesByGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await Context.AzureResources
            .Where(r => r.ResourceGroupId == resourceGroupId)
            .Select(r => new ResourceSummary(
                r.Id.Value,
                r.Name.Value,
                r.ResourceType,
                r.Location.Value.ToString(),
                r.IsExisting,
                r.CustomNameOverride))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Dictionary<Guid, StorageAccountSubResourcesResult>> GetStorageSubResourcesByStorageAccountIdsAsync(
        IReadOnlyList<AzureResourceId> storageAccountIds,
        CancellationToken cancellationToken = default)
    {
        if (storageAccountIds.Count == 0)
            return [];

        var distinctStorageAccountIds = storageAccountIds
            .Distinct()
            .ToList();

        var blobContainers = await Context.Set<BlobContainer>()
            .Where(container => distinctStorageAccountIds.Contains(container.StorageAccountId))
            .AsNoTracking()
            .Select(container => new
            {
                StorageAccountId = container.StorageAccountId.Value,
                BlobContainer = new BlobContainerResult(container.Id, container.Name, container.PublicAccess),
            })
            .ToListAsync(cancellationToken);

        var queues = await Context.Set<StorageQueue>()
            .Where(queue => distinctStorageAccountIds.Contains(queue.StorageAccountId))
            .AsNoTracking()
            .Select(queue => new
            {
                StorageAccountId = queue.StorageAccountId.Value,
                Queue = new StorageQueueResult(queue.Id, queue.Name),
            })
            .ToListAsync(cancellationToken);

        var tables = await Context.Set<StorageTable>()
            .Where(table => distinctStorageAccountIds.Contains(table.StorageAccountId))
            .AsNoTracking()
            .Select(table => new
            {
                StorageAccountId = table.StorageAccountId.Value,
                Table = new StorageTableResult(table.Id, table.Name),
            })
            .ToListAsync(cancellationToken);

        var blobContainersByStorageId = blobContainers
            .GroupBy(item => item.StorageAccountId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<BlobContainerResult>)group.Select(item => item.BlobContainer).ToList());

        var queuesByStorageId = queues
            .GroupBy(item => item.StorageAccountId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<StorageQueueResult>)group.Select(item => item.Queue).ToList());

        var tablesByStorageId = tables
            .GroupBy(item => item.StorageAccountId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<StorageTableResult>)group.Select(item => item.Table).ToList());

        return distinctStorageAccountIds.ToDictionary(
            storageAccountId => storageAccountId.Value,
            storageAccountId => new StorageAccountSubResourcesResult(
                blobContainersByStorageId.GetValueOrDefault(storageAccountId.Value) ?? [],
                queuesByStorageId.GetValueOrDefault(storageAccountId.Value) ?? [],
                tablesByStorageId.GetValueOrDefault(storageAccountId.Value) ?? []));
    }

    /// <inheritdoc />
    public async Task<List<string>> GetDistinctResourceTypesByProjectIdAsync(
        ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        var configIds = Context.Set<InfrastructureConfig>()
            .Where(ic => ic.ProjectId == projectId)
            .Select(ic => ic.Id);

        var resourceGroupIds = Context.Set<ResourceGroup>()
            .Where(rg => configIds.Contains(rg.InfraConfigId))
            .Select(rg => rg.Id);

        return await Context.AzureResources
            .Where(r => resourceGroupIds.Contains(r.ResourceGroupId))
            .Select(r => (string)r.ResourceType)
            .Distinct()
            .OrderBy(t => t)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<ResourceMetadata>> GetResourceMetadataBatchAsync(
        IReadOnlyList<AzureResourceId> resourceIds,
        CancellationToken cancellationToken = default)
    {
        if (resourceIds.Count == 0)
            return [];

        var ids = resourceIds.ToList();

        return await Context.AzureResources
            .Where(r => ids.Contains(r.Id))
            .Join(
                Context.Set<ResourceGroup>(),
                r => r.ResourceGroupId,
                rg => rg.Id,
                (r, rg) => new ResourceMetadata(
                    r.Id.Value,
                    r.Name.Value,
                    r.ResourceType,
                    rg.Name.Value))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

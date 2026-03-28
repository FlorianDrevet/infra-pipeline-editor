using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate;
using InfraFlowSculptor.Domain.ApplicationInsightsAggregate;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.KeyVaultAggregate.Entities;
using InfraFlowSculptor.Domain.RedisCacheAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.Entities;
using InfraFlowSculptor.Domain.WebAppAggregate.Entities;
using InfraFlowSculptor.Domain.FunctionAppAggregate.Entities;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Entities;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate.Entities;
using InfraFlowSculptor.Domain.ApplicationInsightsAggregate.Entities;
using InfraFlowSculptor.Domain.CosmosDbAggregate.Entities;
using InfraFlowSculptor.Domain.SqlServerAggregate.Entities;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.Entities;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.Entities;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class ResourceGroupRepository: BaseRepository<ResourceGroup, ProjectDbContext>, IResourceGroupRepository
{
    public ResourceGroupRepository(ProjectDbContext context) : base(context)
    {
    }

    public override async Task<ResourceGroup?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken)
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
        var result = new Dictionary<Guid, Guid>();

        var webApps = await Context.Set<WebApp>()
            .Where(r => r.ResourceGroupId == resourceGroupId)
            .Select(r => new { ChildId = r.Id.Value, ParentId = r.AppServicePlanId.Value })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        foreach (var item in webApps) result[item.ChildId] = item.ParentId;

        var functionApps = await Context.Set<FunctionApp>()
            .Where(r => r.ResourceGroupId == resourceGroupId)
            .Select(r => new { ChildId = r.Id.Value, ParentId = r.AppServicePlanId.Value })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        foreach (var item in functionApps) result[item.ChildId] = item.ParentId;

        var containerApps = await Context.Set<ContainerApp>()
            .Where(r => r.ResourceGroupId == resourceGroupId)
            .Select(r => new { ChildId = r.Id.Value, ParentId = r.ContainerAppEnvironmentId.Value })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        foreach (var item in containerApps) result[item.ChildId] = item.ParentId;

        var sqlDatabases = await Context.Set<SqlDatabase>()
            .Where(r => r.ResourceGroupId == resourceGroupId)
            .Select(r => new { ChildId = r.Id.Value, ParentId = r.SqlServerId.Value })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        foreach (var item in sqlDatabases) result[item.ChildId] = item.ParentId;

        var appInsights = await Context.Set<ApplicationInsights>()
            .Where(r => r.ResourceGroupId == resourceGroupId)
            .Select(r => new { ChildId = r.Id.Value, ParentId = r.LogAnalyticsWorkspaceId.Value })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        foreach (var item in appInsights) result[item.ChildId] = item.ParentId;

        return result;
    }

    public async Task<ResourceGroup?> GetByResourceIdAsync(
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
        var result = new Dictionary<Guid, List<string>>();

        // Get all resource IDs in this resource group as raw Guid values
        var resourceIds = await Context.Set<AzureResource>()
            .AsNoTracking()
            .Where(r => r.ResourceGroupId == resourceGroupId)
            .Select(r => r.Id.Value)
            .ToListAsync(cancellationToken);

        if (resourceIds.Count == 0) return result;

        // Build a subquery (not materialized) for use in server-side joins
        var resourceIdsQuery = Context.Set<AzureResource>()
            .Where(r => r.ResourceGroupId == resourceGroupId)
            .Select(r => r.Id);

        // Query each typed environment settings table
        await CollectEnvNamesAsync(
            Context.Set<KeyVaultEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.KeyVaultId))
                .Select(es => new ResourceEnvironmentEntry(es.KeyVaultId.Value, es.EnvironmentName)),
            result, cancellationToken);

        await CollectEnvNamesAsync(
            Context.Set<RedisCacheEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.RedisCacheId))
                .Select(es => new ResourceEnvironmentEntry(es.RedisCacheId.Value, es.EnvironmentName)),
            result, cancellationToken);

        await CollectEnvNamesAsync(
            Context.Set<StorageAccountEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.StorageAccountId))
                .Select(es => new ResourceEnvironmentEntry(es.StorageAccountId.Value, es.EnvironmentName)),
            result, cancellationToken);

        await CollectEnvNamesAsync(
            Context.Set<AppServicePlanEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.AppServicePlanId))
                .Select(es => new ResourceEnvironmentEntry(es.AppServicePlanId.Value, es.EnvironmentName)),
            result, cancellationToken);

        await CollectEnvNamesAsync(
            Context.Set<WebAppEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.WebAppId))
                .Select(es => new ResourceEnvironmentEntry(es.WebAppId.Value, es.EnvironmentName)),
            result, cancellationToken);

        await CollectEnvNamesAsync(
            Context.Set<FunctionAppEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.FunctionAppId))
                .Select(es => new ResourceEnvironmentEntry(es.FunctionAppId.Value, es.EnvironmentName)),
            result, cancellationToken);

        await CollectEnvNamesAsync(
            Context.Set<AppConfigurationEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.AppConfigurationId))
                .Select(es => new ResourceEnvironmentEntry(es.AppConfigurationId.Value, es.EnvironmentName)),
            result, cancellationToken);

        await CollectEnvNamesAsync(
            Context.Set<ContainerAppEnvironmentEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.ContainerAppEnvironmentId))
                .Select(es => new ResourceEnvironmentEntry(es.ContainerAppEnvironmentId.Value, es.EnvironmentName)),
            result, cancellationToken);

        await CollectEnvNamesAsync(
            Context.Set<ContainerAppEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.ContainerAppId))
                .Select(es => new ResourceEnvironmentEntry(es.ContainerAppId.Value, es.EnvironmentName)),
            result, cancellationToken);

        await CollectEnvNamesAsync(
            Context.Set<LogAnalyticsWorkspaceEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.LogAnalyticsWorkspaceId))
                .Select(es => new ResourceEnvironmentEntry(es.LogAnalyticsWorkspaceId.Value, es.EnvironmentName)),
            result, cancellationToken);

        await CollectEnvNamesAsync(
            Context.Set<ApplicationInsightsEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.ApplicationInsightsId))
                .Select(es => new ResourceEnvironmentEntry(es.ApplicationInsightsId.Value, es.EnvironmentName)),
            result, cancellationToken);

        await CollectEnvNamesAsync(
            Context.Set<CosmosDbEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.CosmosDbId))
                .Select(es => new ResourceEnvironmentEntry(es.CosmosDbId.Value, es.EnvironmentName)),
            result, cancellationToken);

        await CollectEnvNamesAsync(
            Context.Set<SqlServerEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.SqlServerId))
                .Select(es => new ResourceEnvironmentEntry(es.SqlServerId.Value, es.EnvironmentName)),
            result, cancellationToken);

        await CollectEnvNamesAsync(
            Context.Set<SqlDatabaseEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.SqlDatabaseId))
                .Select(es => new ResourceEnvironmentEntry(es.SqlDatabaseId.Value, es.EnvironmentName)),
            result, cancellationToken);

        await CollectEnvNamesAsync(
            Context.Set<ServiceBusNamespaceEnvironmentSettings>()
                .Where(es => resourceIdsQuery.Contains(es.ServiceBusNamespaceId))
                .Select(es => new ResourceEnvironmentEntry(es.ServiceBusNamespaceId.Value, es.EnvironmentName)),
            result, cancellationToken);

        return result;
    }

    /// <summary>
    /// Collects environment names from a projected query into the result dictionary.
    /// </summary>
    private static async Task CollectEnvNamesAsync(
        IQueryable<ResourceEnvironmentEntry> query,
        Dictionary<Guid, List<string>> result,
        CancellationToken cancellationToken)
    {
        var items = await query.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            if (!result.TryGetValue(item.ResourceId, out var list))
            {
                list = new List<string>();
                result[item.ResourceId] = list;
            }

            if (!list.Contains(item.EnvironmentName))
                list.Add(item.EnvironmentName);
        }
    }

    /// <summary>Projection DTO for environment settings queries.</summary>
    private sealed record ResourceEnvironmentEntry(Guid ResourceId, string EnvironmentName);
}
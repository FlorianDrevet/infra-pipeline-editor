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
}
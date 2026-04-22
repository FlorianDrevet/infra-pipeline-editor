using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
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
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
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
using InfraFlowSculptor.Domain.ContainerRegistryAggregate.Entities;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;
using InfraFlowSculptor.Infrastructure.Persistence.Views;
using InfraFlowSculptor.Application.ResourceGroups.Common;

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
        var links = await Context.ChildToParentLinkViews
            .Where(l => l.ResourceGroupId == resourceGroupId.Value)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return links.ToDictionary(l => l.ChildResourceId, l => l.ParentResourceId);
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
        var entries = await Context.ResourceEnvironmentEntryViews
            .Where(e => e.ResourceGroupId == resourceGroupId.Value)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

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
            .Select(r => r.ResourceType)
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
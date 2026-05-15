using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository implementation for the <see cref="LogAnalyticsWorkspace"/> aggregate.
/// </summary>
public sealed class LogAnalyticsWorkspaceRepository(ProjectDbContext context)
    : AzureResourceRepository<LogAnalyticsWorkspace>(context), ILogAnalyticsWorkspaceRepository
{
    /// <inheritdoc />
    public override async Task<LogAnalyticsWorkspace?> GetByIdAsync(
        ValueObject id,
        CancellationToken cancellationToken)
    {
        return await WithSubResources(Context.Set<LogAnalyticsWorkspace>())
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<LogAnalyticsWorkspace>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<LogAnalyticsWorkspace>())
            .Where(x => x.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<LogAnalyticsWorkspace> WithSubResources(IQueryable<LogAnalyticsWorkspace> query)
    {
        return query
            .Include(x => x.DependsOn)
            .Include(x => x.EnvironmentSettings);
    }
}

using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository implementation for the <see cref="ContainerRegistry"/> aggregate.
/// </summary>
public sealed class ContainerRegistryRepository(ProjectDbContext context)
    : AzureResourceRepository<ContainerRegistry>(context), IContainerRegistryRepository
{
    /// <inheritdoc />
    public override async Task<ContainerRegistry?> GetByIdAsync(
        ValueObject id,
        CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<ContainerRegistry>())
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<ContainerRegistry?> GetByIdReadOnlyAsync(
        ValueObject id,
        CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<ContainerRegistry>().AsNoTracking())
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<ContainerRegistry>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<ContainerRegistry>())
            .Where(x => x.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<ContainerRegistry> WithSubResources(IQueryable<ContainerRegistry> query)
    {
        return query
            .Include(x => x.DependsOn)
            .Include(x => x.EnvironmentSettings);
    }
}

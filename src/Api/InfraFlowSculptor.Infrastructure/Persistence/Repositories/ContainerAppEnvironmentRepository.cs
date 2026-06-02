using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository implementation for the <see cref="ContainerAppEnvironment"/> aggregate.
/// </summary>
public sealed class ContainerAppEnvironmentRepository(ProjectDbContext context)
    : AzureResourceRepository<ContainerAppEnvironment>(context), IContainerAppEnvironmentRepository
{
    /// <inheritdoc />
    public override async Task<ContainerAppEnvironment?> GetByIdAsync(
        ValueObject id,
        CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<ContainerAppEnvironment>())
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<ContainerAppEnvironment?> GetByIdReadOnlyAsync(
        ValueObject id,
        CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<ContainerAppEnvironment>().AsNoTracking())
            .Include(x => x.ResourceGroup)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<ContainerAppEnvironment>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<ContainerAppEnvironment>())
            .Where(x => x.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<ContainerAppEnvironment> WithSubResources(IQueryable<ContainerAppEnvironment> query)
    {
        return query
            .Include(x => x.DependsOn)
            .Include(x => x.EnvironmentSettings);
    }
}

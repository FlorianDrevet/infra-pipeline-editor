using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository implementation for the <see cref="AppConfiguration"/> aggregate.
/// </summary>
public sealed class AppConfigurationRepository : AzureResourceRepository<AppConfiguration>, IAppConfigurationRepository
{
    /// <summary>Initializes a new instance of the <see cref="AppConfigurationRepository"/> class.</summary>
    /// <param name="context">The database context.</param>
    public AppConfigurationRepository(ProjectDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public override async Task<AppConfiguration?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken)
    {
        return await WithSubResources(Context.Set<AppConfiguration>())
            .FirstOrDefaultAsync(ac => ac.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<AppConfiguration>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<AppConfiguration>())
            .Where(ac => ac.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AppConfiguration?> GetByIdWithConfigurationKeysAsync(
        AzureResourceId id,
        CancellationToken cancellationToken)
    {
        return await WithConfigurationKeys(Context.Set<AppConfiguration>())
            .FirstOrDefaultAsync(ac => ac.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AppConfiguration?> GetByIdWithConfigurationKeysAndRoleAssignmentsAsync(
        AzureResourceId id,
        CancellationToken cancellationToken)
    {
        return await WithConfigurationKeys(Context.Set<AppConfiguration>())
            .Include(ac => ac.RoleAssignments)
            .FirstOrDefaultAsync(ac => ac.Id == id, cancellationToken);
    }

    private static IQueryable<AppConfiguration> WithSubResources(IQueryable<AppConfiguration> query)
    {
        return WithConfigurationKeys(query)
            .Include(ac => ac.DependsOn)
            .Include(ac => ac.EnvironmentSettings);
    }

    private static IQueryable<AppConfiguration> WithConfigurationKeys(IQueryable<AppConfiguration> query)
    {
        return query
            .Include(ac => ac.ConfigurationKeys)
                .ThenInclude(ck => ck.EnvironmentValues);
    }
}

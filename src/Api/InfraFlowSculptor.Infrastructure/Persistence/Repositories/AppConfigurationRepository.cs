using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
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
        return await Context.Set<AppConfiguration>()
            .Include(ac => ac.DependsOn)
            .Include(ac => ac.EnvironmentSettings)
            .FirstOrDefaultAsync(ac => ac.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<AppConfiguration>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<AppConfiguration>()
            .Include(ac => ac.DependsOn)
            .Include(ac => ac.EnvironmentSettings)
            .Where(ac => ac.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

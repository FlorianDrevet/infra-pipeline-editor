using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IWebAppRepository"/>.</summary>
public class WebAppRepository(ProjectDbContext context)
    : AzureResourceRepository<WebApp>(context), IWebAppRepository
{
    /// <inheritdoc />
    public override async Task<WebApp?> GetByIdAsync(
        ValueObject id,
        CancellationToken cancellationToken)
    {
        return await Context.Set<WebApp>()
            .Include(x => x.DependsOn)
            .Include(x => x.EnvironmentSettings)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<WebApp>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<WebApp>()
            .Include(x => x.DependsOn)
            .Include(x => x.EnvironmentSettings)
            .Where(x => x.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<WebApp>> GetByAppServicePlanIdAsync(
        AzureResourceId appServicePlanId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<WebApp>()
            .Where(x => x.AppServicePlanId == appServicePlanId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

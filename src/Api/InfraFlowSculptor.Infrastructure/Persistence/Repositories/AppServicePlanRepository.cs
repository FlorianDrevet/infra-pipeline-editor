using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IAppServicePlanRepository"/>.</summary>
public class AppServicePlanRepository(ProjectDbContext context)
    : AzureResourceRepository<AppServicePlan>(context), IAppServicePlanRepository
{
    /// <inheritdoc />
    public override async Task<AppServicePlan?> GetByIdAsync(
        ValueObject id,
        CancellationToken cancellationToken)
    {
        return await Context.Set<AppServicePlan>()
            .Include(x => x.DependsOn)
            .Include(x => x.EnvironmentSettings)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<AppServicePlan>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<AppServicePlan>()
            .Include(x => x.DependsOn)
            .Include(x => x.EnvironmentSettings)
            .Where(x => x.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

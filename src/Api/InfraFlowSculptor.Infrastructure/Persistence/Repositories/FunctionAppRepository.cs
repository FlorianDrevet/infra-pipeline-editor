using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IFunctionAppRepository"/>.</summary>
public sealed class FunctionAppRepository(ProjectDbContext context)
    : AzureResourceRepository<FunctionApp>(context), IFunctionAppRepository
{
    /// <inheritdoc />
    public override async Task<FunctionApp?> GetByIdAsync(
        ValueObject id,
        CancellationToken cancellationToken)
    {
        return await Context.Set<FunctionApp>()
            .Include(x => x.DependsOn)
            .Include(x => x.EnvironmentSettings)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FunctionApp>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<FunctionApp>()
            .Include(x => x.DependsOn)
            .Include(x => x.EnvironmentSettings)
            .Where(x => x.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FunctionApp>> GetByAppServicePlanIdAsync(
        AzureResourceId appServicePlanId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<FunctionApp>()
            .Where(x => x.AppServicePlanId == appServicePlanId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>
/// Provides persistence operations for the <see cref="AppServicePlan"/> aggregate root.
/// </summary>
public interface IAppServicePlanRepository : IRepository<AppServicePlan>
{
    /// <summary>
    /// Retrieves all App Service Plans in the given Resource Group.
    /// </summary>
    Task<List<AppServicePlan>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}

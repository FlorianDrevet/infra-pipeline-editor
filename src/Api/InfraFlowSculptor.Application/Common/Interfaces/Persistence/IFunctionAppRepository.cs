using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>
/// Provides persistence operations for the <see cref="FunctionApp"/> aggregate root.
/// </summary>
public interface IFunctionAppRepository : IRepository<FunctionApp>
{
    /// <summary>
    /// Retrieves all Function Apps in the given Resource Group.
    /// </summary>
    Task<List<FunctionApp>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}

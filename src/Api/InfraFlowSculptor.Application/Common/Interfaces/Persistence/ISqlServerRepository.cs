using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.SqlServerAggregate;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>
/// Provides persistence operations for the <see cref="SqlServer"/> aggregate root.
/// </summary>
public interface ISqlServerRepository : IRepository<SqlServer>
{
    /// <summary>
    /// Retrieves all SQL Servers in the given Resource Group.
    /// </summary>
    Task<List<SqlServer>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}

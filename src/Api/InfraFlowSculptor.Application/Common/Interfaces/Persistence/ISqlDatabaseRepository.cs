using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>
/// Provides persistence operations for the <see cref="SqlDatabase"/> aggregate root.
/// </summary>
public interface ISqlDatabaseRepository : IRepository<SqlDatabase>
{
    /// <summary>
    /// Retrieves all SQL Databases in the given Resource Group.
    /// </summary>
    Task<List<SqlDatabase>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}

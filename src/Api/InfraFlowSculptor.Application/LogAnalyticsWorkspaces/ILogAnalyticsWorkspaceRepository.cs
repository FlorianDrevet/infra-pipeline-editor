using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository interface for the <see cref="LogAnalyticsWorkspace"/> aggregate.</summary>
public interface ILogAnalyticsWorkspaceRepository : IRepository<LogAnalyticsWorkspace>
{
    /// <summary>Retrieves all Log Analytics Workspaces belonging to the specified resource group.</summary>
    Task<List<LogAnalyticsWorkspace>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}

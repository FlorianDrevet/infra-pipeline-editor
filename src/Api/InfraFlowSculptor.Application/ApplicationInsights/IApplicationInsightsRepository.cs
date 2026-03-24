using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using ApplicationInsightsEntity = InfraFlowSculptor.Domain.ApplicationInsightsAggregate.ApplicationInsights;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository interface for the <see cref="ApplicationInsightsEntity"/> aggregate.</summary>
public interface IApplicationInsightsRepository : IRepository<ApplicationInsightsEntity>
{
    /// <summary>Retrieves all Application Insights resources belonging to the specified resource group.</summary>
    Task<List<ApplicationInsightsEntity>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all Application Insights resources linked to the specified Log Analytics Workspace.</summary>
    Task<List<ApplicationInsightsEntity>> GetByLogAnalyticsWorkspaceIdAsync(AzureResourceId logAnalyticsWorkspaceId, CancellationToken cancellationToken = default);
}

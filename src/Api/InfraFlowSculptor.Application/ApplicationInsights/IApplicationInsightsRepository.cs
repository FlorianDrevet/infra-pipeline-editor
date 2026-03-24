using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using ApplicationInsightsEntity = InfraFlowSculptor.Domain.ApplicationInsightsAggregate.ApplicationInsights;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository interface for the <see cref="ApplicationInsightsEntity"/> aggregate.</summary>
public interface IApplicationInsightsRepository : IRepository<ApplicationInsightsEntity>
{
    /// <summary>Retrieves all Application Insights resources belonging to the specified resource group.</summary>
    Task<List<ApplicationInsightsEntity>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}

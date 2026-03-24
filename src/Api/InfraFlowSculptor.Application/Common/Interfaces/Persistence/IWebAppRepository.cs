using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>
/// Provides persistence operations for the <see cref="WebApp"/> aggregate root.
/// </summary>
public interface IWebAppRepository : IRepository<WebApp>
{
    /// <summary>
    /// Retrieves all Web Apps in the given Resource Group.
    /// </summary>
    Task<List<WebApp>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all Web Apps linked to the specified App Service Plan.
    /// </summary>
    Task<List<WebApp>> GetByAppServicePlanIdAsync(
        AzureResourceId appServicePlanId, CancellationToken cancellationToken = default);
}

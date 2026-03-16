using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IAzureResourceRepository
{
    Task<AzureResource?> GetByIdAsync(AzureResourceId id, CancellationToken cancellationToken = default);
    Task<AzureResource?> GetByIdWithRoleAssignmentsAsync(AzureResourceId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(AzureResourceId id, CancellationToken cancellationToken = default);
    Task<AzureResource> UpdateAsync(AzureResource resource, CancellationToken cancellationToken = default);
}

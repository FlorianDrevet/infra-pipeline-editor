using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IKeyVaultRepository: IRepository<KeyVault>
{
    Task<List<KeyVault>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}
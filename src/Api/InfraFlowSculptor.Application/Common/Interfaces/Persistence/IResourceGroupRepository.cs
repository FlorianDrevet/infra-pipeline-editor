using Shared.Application.Interfaces;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IResourceGroupRepository: IRepository<Domain.ResourceGroupAggregate.ResourceGroup>
{
    
}
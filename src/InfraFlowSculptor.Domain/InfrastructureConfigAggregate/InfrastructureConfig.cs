using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate;

public sealed class InfrastructureConfig : AggregateRoot<InfrastructureConfigId>
{
    public Name Name { get; private set; } = null!;
    public List<ResourceGroup> ResourceGroups { get; set; } = new();
    //public List<EnvironmentVariable> Variables { get; set; } = new();

    private InfrastructureConfig(InfrastructureConfigId id)
        : base(id)
    {

    }

    public static InfrastructureConfig Create()
    {
        return new InfrastructureConfig(InfrastructureConfigId.CreateUnique());
    }

    public InfrastructureConfig()
    {
    }
}
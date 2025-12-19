using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public sealed class TenantId: SingleValueObject<Guid>
{
    private TenantId() { }

    public TenantId(Guid value) : base(value)
    {
    }
}
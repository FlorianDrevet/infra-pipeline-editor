using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public sealed class TenantId : SingleValueObject<Guid>
{
    private TenantId() { }

    public TenantId(Guid value) : base(value)
    {
    }
}

using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

public sealed class EntraId: SingleValueObject<Guid>
{
    private EntraId() { }

    public EntraId(Guid value) : base(value)
    {
    }
}
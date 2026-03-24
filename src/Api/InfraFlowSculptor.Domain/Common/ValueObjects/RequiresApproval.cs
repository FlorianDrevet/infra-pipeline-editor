using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public sealed class RequiresApproval : SingleValueObject<bool>
{
    private RequiresApproval() { }

    public RequiresApproval(bool value) : base(value)
    {
    }
}

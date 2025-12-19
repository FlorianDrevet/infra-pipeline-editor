using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public sealed class RequiresApproval: SingleValueObject<bool>
{
    private RequiresApproval() { }

    public RequiresApproval(bool value) : base(value)
    {
    }
}
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

/// <summary>Indicates whether a deployment environment requires manual approval before provisioning.</summary>
public sealed class RequiresApproval : SingleValueObject<bool>
{
    private RequiresApproval() { }

    public RequiresApproval(bool value) : base(value)
    {
    }
}

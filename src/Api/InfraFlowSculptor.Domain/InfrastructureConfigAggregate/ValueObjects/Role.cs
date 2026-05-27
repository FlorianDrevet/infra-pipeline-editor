using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

/// <summary>Role assigned to a member within a project (Owner, Contributor, or Reader).</summary>
public sealed class Role(Role.RoleEnum value) : EnumValueObject<Role.RoleEnum>(value)
{
    /// <summary>Available membership roles.</summary>
    public enum RoleEnum // NOSONAR S2344
    {
        Owner,
        Contributor,
        Reader,
    }
}

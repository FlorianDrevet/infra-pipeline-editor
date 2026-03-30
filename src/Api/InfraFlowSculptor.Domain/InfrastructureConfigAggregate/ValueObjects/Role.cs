using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

/// <summary>Role assigned to a member within a project (Owner, Contributor, or Reader).</summary>
public class Role(Role.RoleEnum value) : EnumValueObject<Role.RoleEnum>(value)
{
    /// <summary>Available membership roles.</summary>
    public enum RoleEnum
    {
        Owner,
        Contributor,
        Reader,
    }
}
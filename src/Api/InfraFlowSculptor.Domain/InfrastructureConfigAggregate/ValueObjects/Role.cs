using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public class Role(Role.RoleEnum value) : EnumValueObject<Role.RoleEnum>(value)
{
    public enum RoleEnum
    {
        Owner,
        Contributor,
        Reader,
    }
}
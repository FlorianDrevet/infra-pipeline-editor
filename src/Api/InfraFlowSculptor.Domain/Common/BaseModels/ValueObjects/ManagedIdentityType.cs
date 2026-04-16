using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>Type of Azure managed identity used for RBAC role assignments.</summary>
public sealed class ManagedIdentityType(ManagedIdentityType.IdentityTypeEnum value) : EnumValueObject<ManagedIdentityType.IdentityTypeEnum>(value)
{
    /// <summary>Supported managed identity types.</summary>
    public enum IdentityTypeEnum
    {
        SystemAssigned,
        UserAssigned
    }
}

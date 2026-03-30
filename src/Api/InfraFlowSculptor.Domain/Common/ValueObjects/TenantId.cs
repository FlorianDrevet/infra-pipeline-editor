using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

/// <summary>Azure Active Directory (Entra ID) tenant identifier.</summary>
public sealed class TenantId : SingleValueObject<Guid>
{
    private TenantId() { }

    public TenantId(Guid value) : base(value)
    {
    }
}

using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

/// <summary>Controls public network access for an Azure resource.</summary>
public sealed class PublicNetworkAccess(PublicNetworkAccess.Access value) : EnumValueObject<PublicNetworkAccess.Access>(value)
{
    /// <summary>Available public network access modes.</summary>
    public enum Access
    {
        /// <summary>Public access is allowed.</summary>
        Enabled,
        /// <summary>Public access is denied — only private endpoints can reach the resource.</summary>
        Disabled,
        /// <summary>Access is secured by a network security perimeter.</summary>
        SecuredByPerimeter
    }
}

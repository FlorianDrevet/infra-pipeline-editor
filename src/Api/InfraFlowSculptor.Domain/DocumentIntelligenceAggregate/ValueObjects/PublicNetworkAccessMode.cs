using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.ValueObjects;

/// <summary>Controls whether public network access is allowed for the resource.</summary>
public sealed class PublicNetworkAccessMode(PublicNetworkAccessMode.Mode value) : EnumValueObject<PublicNetworkAccessMode.Mode>(value)
{
    /// <summary>Available public network access modes.</summary>
    public enum Mode
    {
        /// <summary>Public network access is allowed.</summary>
        Enabled,

        /// <summary>Public network access is disabled (Private Endpoint required).</summary>
        Disabled
    }
}

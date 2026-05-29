using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>
/// Defines how DNS records are managed for a resource private endpoint.
/// </summary>
public sealed class PrivateEndpointDnsMode(PrivateEndpointDnsMode.Mode value) : EnumValueObject<PrivateEndpointDnsMode.Mode>(value)
{
    /// <summary>
    /// Available private endpoint DNS management modes.
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// InfraFlowSculptor manages Private DNS Zones and virtual network links.
        /// </summary>
        AutoManaged,

        /// <summary>
        /// DNS zones are managed in an existing centralized hub.
        /// </summary>
        ExistingHub,

        /// <summary>
        /// DNS is managed outside InfraFlowSculptor.
        /// </summary>
        Disabled
    }
}
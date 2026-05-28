using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;

/// <summary>Defines how Private DNS Zones are managed for private endpoint resolution.</summary>
public sealed class DnsMode(DnsMode.Mode value) : EnumValueObject<DnsMode.Mode>(value)
{
    /// <summary>Available DNS management strategies.</summary>
    public enum Mode
    {
        /// <summary>IFS creates and manages DNS zones alongside the PE resources.</summary>
        AutoManaged,

        /// <summary>DNS zones are centralized in a shared hub resource group (enterprise pattern).</summary>
        CentralizedHub,

        /// <summary>Full manual control over DNS zone references.</summary>
        Custom
    }
}

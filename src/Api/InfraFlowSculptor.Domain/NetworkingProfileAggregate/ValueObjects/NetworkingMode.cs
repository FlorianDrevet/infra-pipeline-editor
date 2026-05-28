using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;

/// <summary>Defines the networking complexity mode for a project.</summary>
public sealed class NetworkingMode(NetworkingMode.Mode value) : EnumValueObject<NetworkingMode.Mode>(value)
{
    /// <summary>Available networking modes.</summary>
    public enum Mode
    {
        /// <summary>Single toggle: privatize all compatible resources. VNet auto-created. DNS auto-managed.</summary>
        Simplified,

        /// <summary>Per-resource privatization selection. VNet existing or new. Optional DNS hub.</summary>
        Standard,

        /// <summary>Full control: per-PE subnet, static IPs, custom DNS zones, multi-region.</summary>
        Advanced
    }
}

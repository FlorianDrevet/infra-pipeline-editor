using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;

/// <summary>Defines how the VNet is sourced for private endpoint deployment.</summary>
public sealed class VnetSource(VnetSource.SourceType value) : EnumValueObject<VnetSource.SourceType>(value)
{
    /// <summary>Available VNet sourcing strategies.</summary>
    public enum SourceType
    {
        /// <summary>IFS generates a new VNet with auto-calculated address space.</summary>
        CreateNew,

        /// <summary>Reference an existing VNet already deployed in Azure.</summary>
        UseExisting,

        /// <summary>Hub-and-spoke topology: VNet managed by the platform team.</summary>
        UseHubSpoke
    }
}

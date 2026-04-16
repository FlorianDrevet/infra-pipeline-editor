using InfraFlowSculptor.Contracts.Common.Requests;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request to set configuration-level tags (full replacement).</summary>
public class SetInfraConfigTagsRequest
{
    /// <summary>Gets the tags to apply to the configuration.</summary>
    public IReadOnlyCollection<TagRequest> Tags { get; init; } = [];
}

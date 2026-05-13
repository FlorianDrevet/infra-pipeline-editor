using InfraFlowSculptor.Contracts.Common.Requests;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request to set configuration-level tags (full replacement).</summary>
public class SetInfraConfigTagsRequest
{
    /// <summary>Gets the tags to apply to the configuration.</summary>
    [MaxCollectionCount(TagRequestConstraints.MaxTagCount)]
    public IReadOnlyList<TagRequest> Tags { get; init; } = [];
}

using InfraFlowSculptor.Contracts.Common.Requests;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request to set project-level tags (full replacement).</summary>
public class SetProjectTagsRequest
{
    /// <summary>Gets the tags to apply to the project.</summary>
    [MaxCollectionCount(TagRequestConstraints.MaxTagCount)]
    public IReadOnlyList<TagRequest> Tags { get; init; } = [];
}

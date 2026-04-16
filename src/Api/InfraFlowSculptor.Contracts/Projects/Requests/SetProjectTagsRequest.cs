using InfraFlowSculptor.Contracts.Common.Requests;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request to set project-level tags (full replacement).</summary>
public class SetProjectTagsRequest
{
    /// <summary>Gets the tags to apply to the project.</summary>
    public IReadOnlyCollection<TagRequest> Tags { get; init; } = [];
}

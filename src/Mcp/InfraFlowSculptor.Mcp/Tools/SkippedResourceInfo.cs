namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>Info about a resource that was skipped during creation.</summary>
public sealed class SkippedResourceInfo
{
    /// <summary>Gets the resource type identifier.</summary>
    public required string ResourceType { get; init; }

    /// <summary>Gets the resource display name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the reason the resource was skipped.</summary>
    public required string Reason { get; init; }
}

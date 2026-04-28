namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>Input for resource creation in the project setup orchestration.</summary>
public sealed class ResourceInput
{
    /// <summary>Gets the resource type identifier.</summary>
    public required string ResourceType { get; init; }

    /// <summary>Gets the resource display name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets optional extracted properties for overriding default values.</summary>
    public IReadOnlyDictionary<string, object?>? ExtractedProperties { get; init; }
}

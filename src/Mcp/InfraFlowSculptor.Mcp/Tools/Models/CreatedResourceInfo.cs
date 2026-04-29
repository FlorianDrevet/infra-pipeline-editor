namespace InfraFlowSculptor.Mcp.Tools.Models;

/// <summary>Info about a successfully created resource.</summary>
public sealed class CreatedResourceInfo
{
    /// <summary>Gets the resource type identifier.</summary>
    public required string ResourceType { get; init; }

    /// <summary>Gets the created resource identifier.</summary>
    public required string ResourceId { get; init; }

    /// <summary>Gets the resource display name.</summary>
    public required string Name { get; init; }
}

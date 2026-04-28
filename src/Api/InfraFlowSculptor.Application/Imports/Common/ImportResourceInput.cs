namespace InfraFlowSculptor.Application.Imports.Common;

/// <summary>
/// Input for resource creation in the import apply flow.
/// </summary>
internal sealed class ImportResourceInput
{
    /// <summary>Gets the resource type identifier.</summary>
    public required string ResourceType { get; init; }

    /// <summary>Gets the resource display name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the Azure location to use for the resource.</summary>
    public string? Location { get; init; }

    /// <summary>Gets the names of resources this resource depends on.</summary>
    public IReadOnlyList<string> DependencyResourceNames { get; init; } = [];

    /// <summary>Gets optional extracted properties for overriding default values.</summary>
    public IReadOnlyDictionary<string, object?>? ExtractedProperties { get; init; }
}

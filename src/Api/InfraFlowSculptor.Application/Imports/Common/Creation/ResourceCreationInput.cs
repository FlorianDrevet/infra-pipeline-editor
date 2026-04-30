namespace InfraFlowSculptor.Application.Imports.Common.Creation;

/// <summary>
/// Common input for resource creation consumed by <see cref="ResourceCreationCoordinator"/>.
/// Both the import-apply flow and the MCP project setup flow map their specific input types to this contract.
/// </summary>
public sealed class ResourceCreationInput
{
    /// <summary>Azure resource type identifier (from <c>AzureResourceTypes</c>).</summary>
    public required string ResourceType { get; init; }

    /// <summary>Desired resource name.</summary>
    public required string Name { get; init; }

    /// <summary>Target Azure location (nullable — defaults to WestEurope).</summary>
    public string? Location { get; init; }

    /// <summary>Explicit dependency resource names for named resolution.</summary>
    public IReadOnlyList<string>? DependencyResourceNames { get; init; }

    /// <summary>Extracted properties dictionary (bridged to typed records internally).</summary>
    public IReadOnlyDictionary<string, object?>? ExtractedProperties { get; init; }
}

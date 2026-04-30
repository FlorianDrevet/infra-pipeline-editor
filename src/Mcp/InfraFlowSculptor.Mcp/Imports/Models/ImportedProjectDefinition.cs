namespace InfraFlowSculptor.Mcp.Imports.Models;

/// <summary>Canonical representation of an imported IaC project definition.</summary>
public sealed record ImportedProjectDefinition
{
    /// <summary>Source format identifier (e.g. "arm-json").</summary>
    public required string SourceFormat { get; init; }

    /// <summary>Parsed resources from the source.</summary>
    public required IReadOnlyList<ImportedResourceDefinition> Resources { get; init; }

    /// <summary>Dependencies between resources.</summary>
    public IReadOnlyList<ImportedDependency> Dependencies { get; init; } = [];

    /// <summary>Source-level metadata (e.g. schema version, content version).</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; }
        = new Dictionary<string, string>();
}

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

/// <summary>A single resource extracted from the IaC source.</summary>
public sealed record ImportedResourceDefinition
{
    /// <summary>ARM resource type string (e.g. "Microsoft.KeyVault/vaults").</summary>
    public required string SourceType { get; init; }

    /// <summary>Resource name from the source template.</summary>
    public required string SourceName { get; init; }

    /// <summary>Mapped InfraFlowSculptor resource type (e.g. "KeyVault"), or null if unsupported.</summary>
    public string? MappedResourceType { get; init; }

    /// <summary>Suggested resource name in IFS, or null if unmapped.</summary>
    public string? MappedName { get; init; }

    /// <summary>Confidence level of the type mapping.</summary>
    public MappingConfidence Confidence { get; init; } = MappingConfidence.High;

    /// <summary>Properties successfully extracted and normalized.</summary>
    public IReadOnlyDictionary<string, object?> ExtractedProperties { get; init; }
        = new Dictionary<string, object?>();

    /// <summary>Source properties that could not be mapped to the IFS model.</summary>
    public IReadOnlyList<string> UnmappedProperties { get; init; } = [];
}

/// <summary>Confidence level for resource type mapping.</summary>
public enum MappingConfidence { High, Medium, Low }

/// <summary>A dependency relationship between two source resources.</summary>
public sealed record ImportedDependency(
    string FromResourceName,
    string ToResourceName,
    string DependencyType);

/// <summary>A gap or issue identified during import analysis.</summary>
public sealed record ImportGap
{
    /// <summary>Severity level of the gap.</summary>
    public required ImportGapSeverity Severity { get; init; }

    /// <summary>Gap category (e.g. "unsupported_resource", "unmapped_property").</summary>
    public required string Category { get; init; }

    /// <summary>Human-readable description of the gap.</summary>
    public required string Message { get; init; }

    /// <summary>The source resource name this gap relates to, if applicable.</summary>
    public string? SourceResourceName { get; init; }
}

/// <summary>Severity levels for import gaps.</summary>
public enum ImportGapSeverity { Info, Warning, Error }

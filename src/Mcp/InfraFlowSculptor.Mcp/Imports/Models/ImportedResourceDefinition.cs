namespace InfraFlowSculptor.Mcp.Imports.Models;

/// <summary>A single resource extracted from the IaC source.</summary>
public sealed record ImportedResourceDefinition
{
    /// <summary>ARM resource type string (e.g. "Microsoft.KeyVault/vaults").</summary>
    public required string SourceType { get; init; }

    /// <summary>Resource name from the source template.</summary>
    public required string SourceName { get; init; }

    /// <summary>Mapped InfraFlowSculptor resource type, or <c>null</c> if unsupported.</summary>
    public string? MappedResourceType { get; init; }

    /// <summary>Suggested resource name in IFS, or <c>null</c> if unmapped.</summary>
    public string? MappedName { get; init; }

    /// <summary>Confidence level of the type mapping.</summary>
    public MappingConfidence Confidence { get; init; } = MappingConfidence.High;

    /// <summary>Properties successfully extracted and normalized.</summary>
    public IReadOnlyDictionary<string, object?> ExtractedProperties { get; init; }
        = new Dictionary<string, object?>();

    /// <summary>Source properties that could not be mapped to the IFS model.</summary>
    public IReadOnlyList<string> UnmappedProperties { get; init; } = [];
}

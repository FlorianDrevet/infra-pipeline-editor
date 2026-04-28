namespace InfraFlowSculptor.Mcp.Imports.Models;

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

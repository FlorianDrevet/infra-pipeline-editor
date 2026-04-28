namespace InfraFlowSculptor.Mcp.Drafts;

/// <summary>A selectable option for a clarification question.</summary>
public sealed class DraftOption
{
    /// <summary>Machine value to set on the field.</summary>
    public required string Value { get; init; }

    /// <summary>Human-readable label.</summary>
    public required string Label { get; init; }

    /// <summary>Optional description of what this option means.</summary>
    public string? Description { get; init; }
}

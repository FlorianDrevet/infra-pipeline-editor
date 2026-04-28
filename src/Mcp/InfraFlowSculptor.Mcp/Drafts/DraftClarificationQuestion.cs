namespace InfraFlowSculptor.Mcp.Drafts;

/// <summary>A clarification question for a missing or ambiguous field.</summary>
public sealed class DraftClarificationQuestion
{
    /// <summary>The field name this question resolves.</summary>
    public required string Field { get; init; }

    /// <summary>Human-readable question text.</summary>
    public required string Message { get; init; }

    /// <summary>Optional pre-defined options for the user to choose from.</summary>
    public List<DraftOption>? Options { get; init; }
}

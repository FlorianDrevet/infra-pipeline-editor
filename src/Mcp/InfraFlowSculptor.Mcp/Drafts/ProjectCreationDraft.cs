namespace InfraFlowSculptor.Mcp.Drafts;

/// <summary>Represents the current state of a conversational project creation draft.</summary>
public sealed class ProjectCreationDraft
{
    /// <summary>Unique draft identifier.</summary>
    public required string DraftId { get; init; }

    /// <summary>Current draft status.</summary>
    public DraftStatus Status { get; set; } = DraftStatus.RequiresClarification;

    /// <summary>Fields that must be provided before creation.</summary>
    public List<string> MissingFields { get; set; } = [];

    /// <summary>Questions to present to the user for missing or ambiguous fields.</summary>
    public List<DraftClarificationQuestion> ClarificationQuestions { get; set; } = [];

    /// <summary>The inferred project intent from the user's prompt.</summary>
    public DraftProjectIntent Intent { get; set; } = new();

    /// <summary>Non-blocking informational messages about defaults or assumptions.</summary>
    public List<string> Warnings { get; set; } = [];

    /// <summary>Blocking validation errors (set during validate).</summary>
    public List<DraftValidationError> Errors { get; set; } = [];

    /// <summary>UTC timestamp when this draft was created. Used for TTL-based cleanup.</summary>
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}

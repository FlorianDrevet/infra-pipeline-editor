namespace InfraFlowSculptor.Mcp.Drafts.Models;

/// <summary>A blocking validation error on a specific field.</summary>
public sealed class DraftValidationError
{
    /// <summary>The field path that has the error.</summary>
    public required string Field { get; init; }

    /// <summary>Human-readable error message.</summary>
    public required string Message { get; init; }
}

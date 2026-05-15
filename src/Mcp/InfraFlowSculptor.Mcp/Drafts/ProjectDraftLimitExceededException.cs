namespace InfraFlowSculptor.Mcp.Drafts;

/// <summary>
/// Represents an error raised when the in-memory draft store has reached its configured capacity.
/// </summary>
public sealed class ProjectDraftLimitExceededException : InvalidOperationException
{
    private const string MessageTemplate = "The in-memory project draft limit of {0} has been reached. Wait for cleanup or remove an existing draft before creating a new one.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectDraftLimitExceededException"/> class.
    /// </summary>
    /// <param name="maxDraftCount">The configured maximum number of drafts allowed in memory.</param>
    public ProjectDraftLimitExceededException(int maxDraftCount)
        : base(string.Format(System.Globalization.CultureInfo.InvariantCulture, MessageTemplate, maxDraftCount))
    {
        MaxDraftCount = maxDraftCount;
    }

    /// <summary>
    /// Gets the configured maximum number of drafts allowed in memory.
    /// </summary>
    public int MaxDraftCount { get; }
}
using System.Text.Json.Serialization;

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
}

/// <summary>Possible states of a project creation draft.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DraftStatus
{
    /// <summary>Draft needs more information before it can be created.</summary>
    RequiresClarification,

    /// <summary>Draft is complete and ready for creation.</summary>
    ReadyToCreate
}

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

/// <summary>The parsed intent from the user's free-form prompt.</summary>
public sealed class DraftProjectIntent
{
    /// <summary>Name of the project to create.</summary>
    public string? ProjectName { get; set; }

    /// <summary>Optional project description.</summary>
    public string? Description { get; set; }

    /// <summary>Repository layout preset (AllInOne, SplitInfraCode, MultiRepo).</summary>
    public string? LayoutPreset { get; set; }

    /// <summary>Inferred environment definitions.</summary>
    public List<DraftEnvironmentIntent>? Environments { get; set; }

    /// <summary>Inferred Azure resource types.</summary>
    public List<DraftResourceIntent>? Resources { get; set; }

    /// <summary>Inferred repository definitions.</summary>
    public List<DraftRepositoryIntent>? Repositories { get; set; }

    /// <summary>Agent pool name for pipeline execution.</summary>
    public string? AgentPoolName { get; set; }

    /// <summary>Pricing intent extracted from the prompt.</summary>
    public string? PricingIntent { get; set; }
}

/// <summary>An inferred environment from the user's prompt.</summary>
public sealed class DraftEnvironmentIntent
{
    /// <summary>Display name of the environment.</summary>
    public string Name { get; set; } = "Development";

    /// <summary>Short identifier for the environment.</summary>
    public string ShortName { get; set; } = "dev";

    /// <summary>Resource name prefix.</summary>
    public string Prefix { get; set; } = "";

    /// <summary>Resource name suffix.</summary>
    public string Suffix { get; set; } = "-dev";

    /// <summary>Azure region key.</summary>
    public string Location { get; set; } = "westeurope";

    /// <summary>Azure subscription identifier; <see cref="Guid.Empty"/> means to configure later.</summary>
    public Guid SubscriptionId { get; set; } = Guid.Empty;

    /// <summary>Deployment order (0-based).</summary>
    public int Order { get; set; }

    /// <summary>Whether this environment requires a deployment approval.</summary>
    public bool RequiresApproval { get; set; }
}

/// <summary>An inferred Azure resource from the user's prompt.</summary>
public sealed class DraftResourceIntent
{
    /// <summary>Resource type identifier matching <c>AzureResourceTypes</c> constants.</summary>
    public required string ResourceType { get; init; }

    /// <summary>Optional resource name.</summary>
    public string? Name { get; set; }

    /// <summary>Optional pricing hint.</summary>
    public string? PricingHint { get; set; }
}

/// <summary>An inferred repository from the user's prompt.</summary>
public sealed class DraftRepositoryIntent
{
    /// <summary>Project-scoped alias for this repository slot.</summary>
    public string Alias { get; set; } = "main";

    /// <summary>Content kinds hosted by this repository.</summary>
    public List<string> ContentKinds { get; set; } = [];

    /// <summary>Optional provider type (GitHub, AzureDevOps).</summary>
    public string? ProviderType { get; set; }

    /// <summary>Optional repository URL.</summary>
    public string? RepositoryUrl { get; set; }

    /// <summary>Optional default branch name.</summary>
    public string? DefaultBranch { get; set; }
}

/// <summary>A blocking validation error on a specific field.</summary>
public sealed class DraftValidationError
{
    /// <summary>The field path that has the error.</summary>
    public required string Field { get; init; }

    /// <summary>Human-readable error message.</summary>
    public required string Message { get; init; }
}

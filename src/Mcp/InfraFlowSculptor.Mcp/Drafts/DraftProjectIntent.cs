using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Mcp.Drafts;

/// <summary>The parsed intent from the user's free-form prompt.</summary>
public sealed class DraftProjectIntent
{
    /// <summary>Name of the project to create.</summary>
    public string? ProjectName { get; set; }

    /// <summary>Optional project description.</summary>
    public string? Description { get; set; }

    /// <summary>Repository layout preset.</summary>
    public LayoutPresetEnum? LayoutPreset { get; set; }

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

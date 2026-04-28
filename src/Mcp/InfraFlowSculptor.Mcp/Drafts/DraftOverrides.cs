namespace InfraFlowSculptor.Mcp.Drafts;

/// <summary>Override values to apply to an existing draft during validation.</summary>
public sealed class DraftOverrides
{
    /// <summary>Override for the project name.</summary>
    public string? ProjectName { get; init; }

    /// <summary>Override for the layout preset.</summary>
    public string? LayoutPreset { get; init; }

    /// <summary>Override for the project description.</summary>
    public string? Description { get; init; }

    /// <summary>Override for environment definitions.</summary>
    public List<DraftEnvironmentIntent>? Environments { get; init; }

    /// <summary>Override for repository definitions.</summary>
    public List<DraftRepositoryIntent>? Repositories { get; init; }

    /// <summary>Override for the agent pool name.</summary>
    public string? AgentPoolName { get; init; }
}

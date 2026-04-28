namespace InfraFlowSculptor.Mcp.Drafts;

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

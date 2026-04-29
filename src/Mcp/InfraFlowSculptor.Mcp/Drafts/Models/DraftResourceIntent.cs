namespace InfraFlowSculptor.Mcp.Drafts.Models;

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

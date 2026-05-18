namespace InfraFlowSculptor.Mcp.Drafts.Models;

/// <summary>Describes which resources belong to which resource group in a multi-RG topology.</summary>
public sealed class DraftResourceGroupAssignment
{
    /// <summary>Logical name of the resource group (e.g. "common", "app").</summary>
    public required string GroupName { get; init; }

    /// <summary>Optional description of the group purpose.</summary>
    public string? Description { get; set; }

    /// <summary>Resource types or resource names assigned to this group.</summary>
    public List<string> ResourceIdentifiers { get; set; } = [];

    /// <summary>Whether this group contains shared/common resources referenced by other groups.</summary>
    public bool IsShared { get; set; }
}

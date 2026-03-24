using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request to validate a list of recently viewed items against current user access.</summary>
public class ValidateRecentItemsRequest
{
    /// <summary>The list of recently viewed item references to validate.</summary>
    [Required]
    public required List<RecentItemEntry> Items { get; init; }
}

/// <summary>A single recently viewed item reference.</summary>
public class RecentItemEntry
{
    /// <summary>The unique identifier of the item.</summary>
    [Required]
    public required string Id { get; init; }

    /// <summary>The type of item: "project" or "config".</summary>
    [Required]
    public required string Type { get; init; }
}

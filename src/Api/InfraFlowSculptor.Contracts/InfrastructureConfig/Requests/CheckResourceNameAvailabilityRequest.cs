using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request body for checking resource name availability.</summary>
public sealed class CheckResourceNameAvailabilityRequest
{
    /// <summary>Project ID owning the environments and naming templates.</summary>
    [Required]
    public required string ProjectId { get; init; }

    /// <summary>Optional infrastructure config ID whose templates may override the project templates.</summary>
    public string? ConfigId { get; init; }

    /// <summary>The user-entered raw name.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>
    /// Optional currently-persisted resource name. When supplied, the handler skips the DNS
    /// check for environments whose generated name matches the persisted one (avoids false
    /// positives on resources the user already owns).
    /// </summary>
    public string? CurrentPersistedName { get; init; }
}

using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>
/// Request to atomically create a project together with its initial layout, environments
/// and (optionally) project-level repositories.
/// </summary>
public sealed class CreateProjectWithSetupRequest
{
    /// <summary>Name of the project to create (3-80 characters).</summary>
    [Required, StringLength(80, MinimumLength = 3)]
    public required string Name { get; init; }

    /// <summary>Optional project description.</summary>
    [StringLength(1000)]
    public string? Description { get; init; }

    /// <summary>Layout preset: <c>AllInOne</c>, <c>SplitInfraCode</c> or <c>MultiRepo</c>.</summary>
    [Required]
    public required string LayoutPreset { get; init; }

    /// <summary>Initial environments (at least one).</summary>
    [Required, MinLength(1)]
    public required IReadOnlyList<EnvironmentSetupRequest> Environments { get; init; }

    /// <summary>Optional project-level repository slots; layout-dependent.</summary>
    public IReadOnlyList<RepositorySetupRequest> Repositories { get; init; } = [];
}

/// <summary>One environment definition for <see cref="CreateProjectWithSetupRequest"/>.</summary>
public sealed class EnvironmentSetupRequest
{
    /// <summary>Display name (e.g. <c>Development</c>).</summary>
    [Required, StringLength(100)]
    public required string Name { get; init; }

    /// <summary>Short identifier (e.g. <c>dev</c>).</summary>
    [Required, StringLength(20)]
    public required string ShortName { get; init; }

    /// <summary>Optional resource name prefix.</summary>
    [StringLength(50)]
    public string? Prefix { get; init; }

    /// <summary>Optional resource name suffix.</summary>
    [StringLength(50)]
    public string? Suffix { get; init; }

    /// <summary>Azure region key (e.g. <c>WestEurope</c>).</summary>
    [Required]
    public required string Location { get; init; }

    /// <summary>Optional Azure subscription ID; <see cref="Guid.Empty"/> means "configure later".</summary>
    public Guid SubscriptionId { get; init; } = Guid.Empty;

    /// <summary>Deployment order (0-based).</summary>
    public int Order { get; init; }

    /// <summary>Whether deployments require an explicit approval.</summary>
    public bool RequiresApproval { get; init; }
}

/// <summary>One project-level repository slot for <see cref="CreateProjectWithSetupRequest"/>.</summary>
public sealed class RepositorySetupRequest
{
    /// <summary>Project-scoped slug (lowercase letters, digits, hyphens), unique per project.</summary>
    [Required, StringLength(50)]
    public required string Alias { get; init; }

    /// <summary>List of content kinds: <c>Infrastructure</c>, <c>ApplicationCode</c>.</summary>
    [Required, MinLength(1)]
    public required IReadOnlyList<string> ContentKinds { get; init; }

    /// <summary>Optional Git provider type.</summary>
    public string? ProviderType { get; init; }

    /// <summary>Optional repository URL.</summary>
    [Url]
    public string? RepositoryUrl { get; init; }

    /// <summary>Optional default branch.</summary>
    [StringLength(200)]
    public string? DefaultBranch { get; init; }
}

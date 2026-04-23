using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>
/// Request to set or clear the repository binding of an infrastructure configuration.
/// Send a <c>null</c> or empty <see cref="RepositoryAlias"/> to clear the binding.
/// </summary>
public sealed class SetInfraConfigRepositoryBindingRequest
{
    /// <summary>The target project repository alias, or <c>null</c> to clear the binding.</summary>
    [StringLength(50)]
    public string? RepositoryAlias { get; init; }

    /// <summary>Optional branch override.</summary>
    [StringLength(200)]
    public string? Branch { get; init; }

    /// <summary>Optional sub-path inside the repository where Bicep files live.</summary>
    [StringLength(500)]
    public string? InfraPath { get; init; }

    /// <summary>Optional sub-path inside the repository where pipeline files live.</summary>
    [StringLength(500)]
    public string? PipelinePath { get; init; }
}

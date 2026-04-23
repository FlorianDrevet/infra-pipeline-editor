namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

/// <summary>
/// Application-layer result describing the optional repository binding of an infrastructure configuration.
/// </summary>
/// <param name="Alias">The alias of the project-level repository this configuration is bound to.</param>
/// <param name="Branch">Optional branch override; when <c>null</c> the repository default branch is used.</param>
/// <param name="InfraPath">Optional sub-path inside the repository where Bicep files live.</param>
/// <param name="PipelinePath">Optional sub-path inside the repository where pipeline files live.</param>
public record RepositoryBindingResult(
    string Alias,
    string? Branch,
    string? InfraPath,
    string? PipelinePath);

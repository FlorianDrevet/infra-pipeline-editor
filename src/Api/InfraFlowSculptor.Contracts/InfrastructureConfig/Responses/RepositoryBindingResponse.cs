namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>
/// Binds an infrastructure configuration to a project-level repository
/// (by alias), with optional per-config overrides.
/// </summary>
/// <param name="Alias">The project repository alias the configuration is bound to.</param>
/// <param name="Branch">Optional branch override.</param>
/// <param name="InfraPath">Optional sub-path inside the repository where Bicep files live.</param>
/// <param name="PipelinePath">Optional sub-path inside the repository where pipeline files live.</param>
public record RepositoryBindingResponse(
    string Alias,
    string? Branch,
    string? InfraPath,
    string? PipelinePath);

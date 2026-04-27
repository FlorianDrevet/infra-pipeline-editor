namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Result of bootstrap pipeline generation at project level.</summary>
/// <param name="FileUris">
/// Bootstrap pipeline files keyed by relative path, returned as a flat union.
/// In <c>SplitInfraCode</c> layout, paths are prefixed with <c>infra/</c> and <c>app/</c>
/// (e.g. <c>infra/bootstrap.pipeline.yml</c>, <c>app/bootstrap.pipeline.yml</c>).
/// In <c>AllInOne</c> layout, paths are root-level (e.g. <c>bootstrap.pipeline.yml</c>).
/// Kept for backward compatibility with clients that consume a single bucket.
/// </param>
/// <param name="InfraFileUris">
/// Bootstrap pipeline files targeted at the infrastructure-flagged repository, keyed by
/// repo-relative path (no <c>infra/</c> prefix). Empty when no infra bootstrap was generated.
/// </param>
/// <param name="AppFileUris">
/// Bootstrap pipeline files targeted at the application-code-flagged repository, keyed by
/// repo-relative path (no <c>app/</c> prefix). Only populated in <c>SplitInfraCode</c> layout.
/// </param>
public record GenerateProjectBootstrapPipelineResponse(
    IReadOnlyDictionary<string, Uri> FileUris,
    IReadOnlyDictionary<string, Uri> InfraFileUris,
    IReadOnlyDictionary<string, Uri> AppFileUris);

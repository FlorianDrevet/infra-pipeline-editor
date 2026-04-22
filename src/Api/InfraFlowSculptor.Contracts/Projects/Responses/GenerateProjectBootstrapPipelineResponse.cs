namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Result of bootstrap pipeline generation at project level.</summary>
/// <param name="FileUris">Bootstrap pipeline files keyed by relative path (e.g. <c>bootstrap.pipeline.yml</c>).</param>
public record GenerateProjectBootstrapPipelineResponse(
    IReadOnlyDictionary<string, Uri> FileUris);

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Response for Azure DevOps pipeline generation.</summary>
/// <param name="FileUris">Map of relative file paths to their blob URIs.</param>
public record GeneratePipelineResponse(
    IReadOnlyDictionary<string, Uri> FileUris);

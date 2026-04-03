namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Response containing URIs to the generated application pipeline files.</summary>
public record GenerateAppPipelineResponse(
    IReadOnlyDictionary<string, Uri> FileUris);

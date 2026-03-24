namespace InfraFlowSculptor.Contracts.AppSettings.Responses;

/// <summary>Response DTO for available outputs from a resource type.</summary>
public record AvailableOutputsResponse(
    string ResourceTypeName,
    IReadOnlyList<OutputDefinitionResponse> Outputs);

/// <summary>Describes a single available output on a resource type.</summary>
public record OutputDefinitionResponse(
    string Name,
    string Description);

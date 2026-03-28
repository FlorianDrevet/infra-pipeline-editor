namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request to generate Azure DevOps pipeline YAML files.</summary>
public record GeneratePipelineRequest(Guid InfrastructureConfigId);

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request to generate the bootstrap pipeline YAML file for an infrastructure configuration.</summary>
public record GenerateBootstrapRequest(Guid InfrastructureConfigId);

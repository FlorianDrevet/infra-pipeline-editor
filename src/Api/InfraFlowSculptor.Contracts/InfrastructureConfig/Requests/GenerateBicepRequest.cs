namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request body for triggering the generation of Azure Bicep files from an existing Infrastructure Configuration.</summary>
/// <param name="InfrastructureConfigId">Unique identifier of the Infrastructure Configuration to generate Bicep files for.</param>
public record GenerateBicepRequest(Guid InfrastructureConfigId);

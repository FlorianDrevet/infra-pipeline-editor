namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Response representing a cross-configuration resource reference.</summary>
public record CrossConfigReferenceResponse(
    string ReferenceId,
    string TargetConfigId,
    string TargetConfigName,
    string TargetResourceId,
    string TargetResourceName,
    string TargetResourceType,
    string TargetResourceGroupName);

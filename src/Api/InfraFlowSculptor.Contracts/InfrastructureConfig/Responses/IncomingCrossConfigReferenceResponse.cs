namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>
/// Response for an incoming cross-config reference: a resource in another configuration
/// that depends on a resource in this configuration.
/// </summary>
public record IncomingCrossConfigReferenceResponse(
    string ReferenceId,
    string SourceConfigId,
    string SourceConfigName,
    string SourceResourceId,
    string SourceResourceName,
    string SourceResourceType,
    string SourceResourceGroupName,
    string TargetResourceId,
    string TargetResourceName,
    string TargetResourceType);

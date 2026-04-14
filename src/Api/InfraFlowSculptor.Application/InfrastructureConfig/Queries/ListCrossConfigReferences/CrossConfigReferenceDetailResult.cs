namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListCrossConfigReferences;

/// <summary>Result for a resolved cross-configuration resource reference.</summary>
public record CrossConfigReferenceDetailResult(
    Guid ReferenceId,
    Guid TargetConfigId,
    string TargetConfigName,
    Guid TargetResourceId,
    string TargetResourceName,
    string TargetResourceType,
    string TargetResourceGroupName);

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListIncomingCrossConfigReferences;

/// <summary>
/// Result for an incoming cross-config reference: a resource in another configuration
/// that references a resource in this configuration.
/// </summary>
/// <param name="ReferenceId">The cross-config reference identifier.</param>
/// <param name="SourceConfigId">The configuration that owns the referencing resource.</param>
/// <param name="SourceConfigName">Name of the source configuration.</param>
/// <param name="SourceResourceId">The resource in the source config that holds the cross-config dependency.</param>
/// <param name="SourceResourceName">Name of the source resource.</param>
/// <param name="SourceResourceType">Type of the source resource.</param>
/// <param name="SourceResourceGroupName">Resource group name of the source resource.</param>
/// <param name="TargetResourceId">The resource in THIS config that is being referenced.</param>
/// <param name="TargetResourceName">Name of the target resource in this config.</param>
/// <param name="TargetResourceType">Type of the target resource in this config.</param>
public record IncomingCrossConfigReferenceResult(
    Guid ReferenceId,
    Guid SourceConfigId,
    string SourceConfigName,
    Guid SourceResourceId,
    string SourceResourceName,
    string SourceResourceType,
    string SourceResourceGroupName,
    Guid TargetResourceId,
    string TargetResourceName,
    string TargetResourceType);

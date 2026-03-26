namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddCrossConfigReference;

/// <summary>Result of adding a cross-configuration resource reference.</summary>
/// <param name="ReferenceId">The identifier of the created reference.</param>
/// <param name="TargetConfigId">The target configuration identifier.</param>
/// <param name="TargetResourceId">The target resource identifier.</param>
public record CrossConfigReferenceResult(Guid ReferenceId, Guid TargetConfigId, Guid TargetResourceId);

using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerApps.Common;

/// <summary>
/// Lightweight Container App detail projection plus authorization context.
/// </summary>
/// <param name="Result">The Container App detail result returned by the query handler after authorization succeeds.</param>
/// <param name="InfraConfigId">The owning infrastructure configuration identifier used for access checks.</param>
public sealed record ContainerAppDetailReadResult(
    ContainerAppResult Result,
    InfrastructureConfigId InfraConfigId);
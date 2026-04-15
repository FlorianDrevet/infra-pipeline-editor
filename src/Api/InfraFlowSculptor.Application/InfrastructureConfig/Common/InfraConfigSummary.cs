namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

/// <summary>Lightweight projection of an infrastructure configuration for list/validation queries.</summary>
/// <param name="Id">The infrastructure configuration identifier as a raw GUID.</param>
/// <param name="Name">The configuration display name.</param>
public sealed record InfraConfigSummary(Guid Id, string Name);

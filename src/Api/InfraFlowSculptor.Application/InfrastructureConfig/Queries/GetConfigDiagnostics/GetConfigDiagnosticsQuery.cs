using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetConfigDiagnostics;

/// <summary>Query to retrieve configuration diagnostics for an infrastructure configuration.</summary>
/// <param name="InfrastructureConfigId">The unique identifier of the configuration to diagnose.</param>
public record GetConfigDiagnosticsQuery(Guid InfrastructureConfigId) : IQuery<GetConfigDiagnosticsResult>;

/// <summary>Result containing all diagnostic findings for the configuration.</summary>
/// <param name="Diagnostics">The list of diagnostic findings.</param>
public record GetConfigDiagnosticsResult(IReadOnlyCollection<ResourceDiagnosticItem> Diagnostics);

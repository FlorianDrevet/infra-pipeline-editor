using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;

/// <summary>Orchestrates all diagnostic rules against a config read model.</summary>
public interface IConfigDiagnosticService
{
    /// <summary>Runs all registered diagnostic rules and returns combined findings.</summary>
    /// <param name="config">The infrastructure configuration read model to evaluate.</param>
    /// <returns>A consolidated list of all diagnostic findings across all rules.</returns>
    IReadOnlyList<ResourceDiagnosticItem> Evaluate(InfrastructureConfigReadModel config);
}

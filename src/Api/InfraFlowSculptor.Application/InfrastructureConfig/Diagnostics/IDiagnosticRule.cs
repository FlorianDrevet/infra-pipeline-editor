using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;

/// <summary>Evaluates a specific diagnostic rule against the full config read model.</summary>
public interface IDiagnosticRule
{
    /// <summary>Evaluates this rule and returns any findings.</summary>
    /// <param name="config">The infrastructure configuration read model to evaluate.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A list of diagnostic findings, empty if the rule passes.</returns>
    Task<IReadOnlyList<ResourceDiagnosticItem>> EvaluateAsync(
        InfrastructureConfigReadModel config,
        CancellationToken cancellationToken = default);
}

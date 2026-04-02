using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;

/// <summary>
/// Orchestrates all registered <see cref="IDiagnosticRule"/> implementations
/// and returns their combined findings.
/// </summary>
public sealed class ConfigDiagnosticService(IEnumerable<IDiagnosticRule> rules) : IConfigDiagnosticService
{
    /// <inheritdoc />
    public IReadOnlyList<ResourceDiagnosticItem> Evaluate(InfrastructureConfigReadModel config)
    {
        return rules
            .SelectMany(rule => rule.Evaluate(config))
            .ToList();
    }
}

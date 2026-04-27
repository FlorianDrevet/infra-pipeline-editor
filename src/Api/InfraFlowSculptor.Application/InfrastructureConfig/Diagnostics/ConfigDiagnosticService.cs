using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;

/// <summary>
/// Orchestrates all registered <see cref="IDiagnosticRule"/> implementations
/// and returns their combined findings.
/// </summary>
public sealed class ConfigDiagnosticService(IEnumerable<IDiagnosticRule> rules) : IConfigDiagnosticService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<ResourceDiagnosticItem>> EvaluateAsync(
        InfrastructureConfigReadModel config,
        CancellationToken cancellationToken = default)
    {
        var tasks = rules.Select(rule => rule.EvaluateAsync(config, cancellationToken));
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        return results
            .SelectMany(items => items)
            .ToList();
    }
}

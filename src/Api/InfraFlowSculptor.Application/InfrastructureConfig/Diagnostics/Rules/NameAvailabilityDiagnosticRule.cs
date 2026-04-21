using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics.Rules;

/// <summary>
/// Checks that resource names are available on Azure for each environment
/// by probing DNS via <see cref="IAzureNameAvailabilityChecker"/>. Because this rule always
/// evaluates the <em>currently-persisted</em> name, a DNS hit is expected after deployment.
/// Such findings are emitted as <see cref="DiagnosticSeverity.Information"/> with an
/// explanatory message instead of a warning.
/// </summary>
public sealed class NameAvailabilityDiagnosticRule(
    IAzureNameAvailabilityChecker nameAvailabilityChecker,
    IResourceNameResolver resourceNameResolver) : IDiagnosticRule
{
    /// <summary>Stable diagnostic code emitted when a resource name resolves on Azure DNS.</summary>
    private const string RuleCode = "NAME_UNAVAILABLE";

    /// <inheritdoc />
    public async Task<IReadOnlyList<ResourceDiagnosticItem>> EvaluateAsync(
        InfrastructureConfigReadModel config,
        CancellationToken cancellationToken = default)
    {
        var diagnostics = new List<ResourceDiagnosticItem>();

        var allResources = config.ResourceGroups
            .SelectMany(rg => rg.Resources)
            .Where(r => nameAvailabilityChecker.Supports(r.ResourceType))
            .ToList();

        if (allResources.Count == 0)
            return diagnostics;

        var projectId = new ProjectId(config.ProjectId);
        var configId = new InfrastructureConfigId(config.Id);

        var tasks = allResources.Select(resource =>
            CheckResourceAsync(config, projectId, configId, resource, cancellationToken));

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        foreach (var items in results)
        {
            diagnostics.AddRange(items);
        }

        return diagnostics;
    }

    /// <summary>
    /// Resolves names for every environment and checks availability against Azure ARM.
    /// </summary>
    private async Task<IReadOnlyList<ResourceDiagnosticItem>> CheckResourceAsync(
        InfrastructureConfigReadModel config,
        ProjectId projectId,
        InfrastructureConfigId configId,
        AzureResourceReadModel resource,
        CancellationToken cancellationToken)
    {
        var diagnostics = new List<ResourceDiagnosticItem>();

        var resolveResult = await resourceNameResolver.ResolveAsync(
            projectId, configId, resource.ResourceType, resource.Name, cancellationToken)
            .ConfigureAwait(false);

        if (resolveResult.IsError)
            return diagnostics;

        var resolvedNames = resolveResult.Value;
        var environmentSubscriptions = config.Environments
            .Where(e => !string.IsNullOrWhiteSpace(e.SubscriptionId))
            .ToDictionary(e => e.Name, e => e.SubscriptionId!, StringComparer.OrdinalIgnoreCase);

        var checkTasks = resolvedNames
            .Where(rn => environmentSubscriptions.ContainsKey(rn.EnvironmentName))
            .Select(async rn =>
            {
                var subscriptionId = environmentSubscriptions[rn.EnvironmentName];
                var result = await nameAvailabilityChecker.CheckAsync(
                    resource.ResourceType, subscriptionId, rn.GeneratedName, cancellationToken)
                    .ConfigureAwait(false);
                return (rn, result);
            });

        var checkResults = await Task.WhenAll(checkTasks).ConfigureAwait(false);

        foreach (var (resolvedName, availability) in checkResults)
        {
            if (availability.Status != AzureNameAvailabilityStatus.Unavailable)
                continue;

            diagnostics.Add(new ResourceDiagnosticItem(
                resource.Id,
                resource.Name,
                resource.ResourceType,
                DiagnosticSeverity.Info,
                RuleCode,
                $"{resolvedName.GeneratedName} ({resolvedName.EnvironmentName}) — name resolves on Azure DNS. Expected if this configuration has already been deployed."));
        }

        return diagnostics;
    }
}

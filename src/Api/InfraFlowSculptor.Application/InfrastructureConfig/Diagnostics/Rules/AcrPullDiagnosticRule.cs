using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics.Rules;

/// <summary>
/// Checks that compute resources in container mode (WebApp, FunctionApp, ContainerApp)
/// have an <c>AcrPull</c> role assignment targeting their referenced container registry.
/// </summary>
public sealed class AcrPullDiagnosticRule : IDiagnosticRule
{
    /// <summary>Stable diagnostic code emitted when an AcrPull assignment is missing.</summary>
    private const string RuleCode = "ACR_PULL_MISSING";

    /// <summary>The property key in <see cref="AzureResourceReadModel.Properties"/> that holds the container registry reference.</summary>
    private const string ContainerRegistryIdProperty = "containerRegistryId";

    /// <summary>ARM resource types considered as container-capable compute resources.</summary>
    private static readonly HashSet<string> ContainerComputeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        AzureResourceTypes.ArmTypes.WebApp,
        AzureResourceTypes.ArmTypes.FunctionApp,
        AzureResourceTypes.ArmTypes.ContainerApp,
    };

    /// <inheritdoc />
    public IReadOnlyList<ResourceDiagnosticItem> Evaluate(InfrastructureConfigReadModel config)
    {
        var allResources = config.ResourceGroups
            .SelectMany(rg => rg.Resources)
            .ToList();

        var diagnostics = new List<ResourceDiagnosticItem>();

        foreach (var resource in allResources)
        {
            if (!ContainerComputeTypes.Contains(resource.ResourceType))
                continue;

            if (!resource.Properties.TryGetValue(ContainerRegistryIdProperty, out var acrIdString)
                || string.IsNullOrWhiteSpace(acrIdString))
                continue;

            if (!Guid.TryParse(acrIdString, out var acrId))
                continue;

            var hasAcrPull = config.RoleAssignments.Any(ra =>
                ra.SourceResourceId == resource.Id
                && ra.TargetResourceId == acrId
                && ra.RoleDefinitionId.Equals(AzureRoleDefinitionCatalog.AcrPull, StringComparison.OrdinalIgnoreCase));

            if (hasAcrPull)
                continue;

            var targetAcrName = allResources
                .FirstOrDefault(r => r.Id == acrId)?.Name ?? acrId.ToString();

            diagnostics.Add(new ResourceDiagnosticItem(
                resource.Id,
                resource.Name,
                resource.ResourceType,
                DiagnosticSeverity.Warning,
                RuleCode,
                targetAcrName));
        }

        return diagnostics;
    }
}

using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
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

    /// <summary>The property key in <see cref="AzureResourceReadModel.Properties"/> that holds the ACR authentication mode.</summary>
    private const string AcrAuthModeProperty = "acrAuthMode";

    /// <summary>The property key in <see cref="AzureResourceReadModel.Properties"/> that holds the dedicated ACR pull identity.</summary>
    private const string AcrPullIdentityIdProperty = "acrPullIdentityId";

    /// <summary>ARM resource types considered as container-capable compute resources.</summary>
    private static readonly HashSet<string> ContainerComputeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        AzureResourceTypes.ArmTypes.WebAppType,
        AzureResourceTypes.ArmTypes.FunctionAppType,
        AzureResourceTypes.ArmTypes.ContainerAppType,
    };

    /// <inheritdoc />
    public Task<IReadOnlyList<ResourceDiagnosticItem>> EvaluateAsync(
        InfrastructureConfigReadModel config,
        CancellationToken cancellationToken = default)
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

            if (resource.Properties.TryGetValue(AcrAuthModeProperty, out var acrAuthMode)
                && string.Equals(
                    acrAuthMode,
                    AcrAuthMode.AcrAuthModeType.AdminCredentials.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!Guid.TryParse(acrIdString, out var acrId))
                continue;

            var hasAcrPull = config.RoleAssignments.Any(ra =>
                ra.SourceResourceId == resource.Id
                && ra.TargetResourceId == acrId
                && ra.RoleDefinitionId.Equals(AzureRoleDefinitionCatalog.AcrPull, StringComparison.OrdinalIgnoreCase));

            // Also check: if the resource specifies an acrPullIdentityId, verify that UAI
            // has an AcrPull role assignment targeting the ACR from any source resource.
            if (!hasAcrPull
                && resource.Properties.TryGetValue(AcrPullIdentityIdProperty, out var acrPullIdentityIdStr)
                && Guid.TryParse(acrPullIdentityIdStr, out var acrPullIdentityId))
            {
                hasAcrPull = config.RoleAssignments.Any(ra =>
                    ra.UserAssignedIdentityResourceId == acrPullIdentityId
                    && ra.TargetResourceId == acrId
                    && ra.RoleDefinitionId.Equals(AzureRoleDefinitionCatalog.AcrPull, StringComparison.OrdinalIgnoreCase));
            }

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

        return Task.FromResult<IReadOnlyList<ResourceDiagnosticItem>>(diagnostics);
    }
}

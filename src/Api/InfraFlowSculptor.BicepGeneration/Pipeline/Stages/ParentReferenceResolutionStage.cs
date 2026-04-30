using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 800 â€” Parent / cross-resource reference resolution.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="BicepGenerationContext.ResourceIdToInfo"/> and
/// <see cref="BicepGenerationContext.WorkItems"/> populated (stage 300).</para>
/// <para><b>Post-conditions:</b> each module's <c>ParentModuleIdReferences</c>,
/// <c>ParentModuleNameReferences</c> and <c>ExistingResourceIdReferences</c> dictionaries are
/// populated based on the resource's foreign-key properties. Unresolvable references (resource
/// not in the same configuration and not in <c>ExistingResourceReferences</c>) are silently dropped.</para>
/// <para>Special-cased fall-backs:</para>
/// <list type="bullet">
///   <item><description><c>Microsoft.Insights/components</c> and <c>Microsoft.App/managedEnvironments</c> auto-detect a Log Analytics Workspace in the same configuration when their <c>logAnalyticsWorkspaceId</c> property is not set, falling back to a cross-config existing reference when no in-config LAW is found.</description></item>
/// </list>
/// </remarks>
public sealed class ParentReferenceResolutionStage : IBicepGenerationStage
{
    /// <inheritdoc />
    public int Order => 800;

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Tracked under test-debt #22: refactoring deferred until dedicated unit-test coverage protects against behavioural regressions. The method orchestrates a single coherent business operation and would lose readability without proper test guards.")]
    public void Execute(BicepGenerationContext context)
    {
        var request = context.Request;
        var resourceIdToInfo = context.ResourceIdToInfo;

        foreach (var item in context.WorkItems)
        {
            var resource = item.Resource;
            var parentModuleIdRefs = new Dictionary<string, (string Name, string ResourceTypeName)>();
            var parentModuleNameRefs = new Dictionary<string, (string Name, string ResourceTypeName)>();
            var existingResourceIdRefs = new Dictionary<string, string>();

            if (resource.Properties.TryGetValue("appServicePlanId", out var aspIdStr)
                && Guid.TryParse(aspIdStr, out var aspGuid)
                && resourceIdToInfo.TryGetValue(aspGuid, out var aspInfo))
            {
                parentModuleIdRefs["appServicePlanId"] = aspInfo;
            }

            if (resource.Properties.TryGetValue("containerAppEnvironmentId", out var caeIdStr)
                && Guid.TryParse(caeIdStr, out var caeGuid)
                && resourceIdToInfo.TryGetValue(caeGuid, out var caeInfo))
            {
                parentModuleIdRefs["containerAppEnvironmentId"] = caeInfo;
            }

            if (resource.Properties.TryGetValue("logAnalyticsWorkspaceId", out var lawIdStr)
                && Guid.TryParse(lawIdStr, out var lawGuid)
                && resourceIdToInfo.TryGetValue(lawGuid, out var lawInfo))
            {
                parentModuleIdRefs["logAnalyticsWorkspaceId"] = lawInfo;
            }
            else if (resource.Type is AzureResourceTypes.ArmTypes.ApplicationInsights or AzureResourceTypes.ArmTypes.ContainerAppEnvironment
                && !parentModuleIdRefs.ContainsKey("logAnalyticsWorkspaceId"))
            {
                var fallbackLaw = request.Resources.FirstOrDefault(r =>
                    r.Type.Equals(AzureResourceTypes.ArmTypes.LogAnalyticsWorkspace, StringComparison.OrdinalIgnoreCase));
                if (fallbackLaw is not null)
                {
                    parentModuleIdRefs["logAnalyticsWorkspaceId"] = (fallbackLaw.Name, AzureResourceTypes.LogAnalyticsWorkspace);
                }
                else
                {
                    var existingLaw = request.ExistingResourceReferences.FirstOrDefault(r =>
                        r.ResourceType.Equals(AzureResourceTypes.ArmTypes.LogAnalyticsWorkspace, StringComparison.OrdinalIgnoreCase));
                    if (existingLaw is not null)
                    {
                        existingResourceIdRefs["logAnalyticsWorkspaceId"] = existingLaw.ResourceName;
                    }
                }
            }

            if (resource.Properties.TryGetValue("sqlServerId", out var sqlIdStr)
                && Guid.TryParse(sqlIdStr, out var sqlGuid)
                && resourceIdToInfo.TryGetValue(sqlGuid, out var sqlInfo))
            {
                parentModuleNameRefs["sqlServerName"] = sqlInfo;
            }

            item.Module = item.Module with
            {
                IdentityKind = item.IdentityKind,
                UsesParameterizedIdentity = item.UsesParameterizedIdentity,
                ParentModuleIdReferences = parentModuleIdRefs,
                ParentModuleNameReferences = parentModuleNameRefs,
                ExistingResourceIdReferences = existingResourceIdRefs,
            };
        }
    }
}

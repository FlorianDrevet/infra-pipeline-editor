using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate;

namespace InfraFlowSculptor.Application.RoleAssignments.Common;

/// <summary>
/// Evaluates the impact of removing a role assignment by checking known dependency patterns
/// between Azure resource types (ACR pull, Key Vault secrets, last-role-to-target).
/// </summary>
public sealed class RoleAssignmentImpactAnalyzer(
    IAzureResourceRepository azureResourceRepository,
    IAppConfigurationRepository appConfigurationRepository)
    : IRoleAssignmentImpactAnalyzer
{
    /// <inheritdoc />
    public async Task<List<RoleAssignmentImpactItem>> AnalyzeAsync(
        AzureResource sourceResource,
        RoleAssignment roleAssignment,
        CancellationToken cancellationToken)
    {
        var impacts = new List<RoleAssignmentImpactItem>();

        var targetResource = await azureResourceRepository.GetByIdAsync(
            roleAssignment.TargetResourceId, cancellationToken);

        if (targetResource is null)
            return impacts;

        var sourceName = sourceResource.Name.Value;
        var sourceType = sourceResource.GetType().Name;
        var targetName = targetResource.Name.Value;
        var targetType = targetResource.GetType().Name;

        AnalyzeAcrPullImpact(sourceResource, roleAssignment, targetResource, sourceName, sourceType, targetName, targetType, impacts);
        await AnalyzeKeyVaultSecretsImpactAsync(sourceResource, roleAssignment, targetResource, sourceName, sourceType, targetName, targetType, impacts, cancellationToken);
        AnalyzeLastRoleToTargetImpact(sourceResource, roleAssignment, sourceName, sourceType, targetName, targetType, impacts);

        return impacts;
    }

    /// <summary>
    /// Rule: If the role being removed is AcrPull and the target is a ContainerRegistry,
    /// and the source is a compute resource (FunctionApp, WebApp, ContainerApp),
    /// this resource may lose the ability to pull its Docker image.
    /// </summary>
    private static void AnalyzeAcrPullImpact(
        AzureResource sourceResource,
        RoleAssignment roleAssignment,
        AzureResource targetResource,
        string sourceName,
        string sourceType,
        string targetName,
        string targetType,
        List<RoleAssignmentImpactItem> impacts)
    {
        if (roleAssignment.RoleDefinitionId != AzureRoleDefinitionCatalog.AcrPull)
            return;

        if (targetResource is not Domain.ContainerRegistryAggregate.ContainerRegistry)
            return;

        if (sourceResource is not (FunctionApp or WebApp or ContainerApp))
            return;

        impacts.Add(new RoleAssignmentImpactItem(
            ImpactType: "AcrPullRequired",
            AffectedResourceId: sourceResource.Id.Value,
            AffectedResourceName: sourceName,
            AffectedResourceType: sourceType,
            TargetResourceId: targetResource.Id.Value,
            TargetResourceName: targetName,
            TargetResourceType: targetType,
            Description: $"'{sourceName}' uses container images from '{targetName}'. Removing AcrPull will prevent it from pulling its Docker image.",
            Severity: "Critical"));
    }

    /// <summary>
    /// Rule: If the role being removed is Key Vault Secrets User, check whether the source resource
    /// has app settings or configuration keys referencing that Key Vault.
    /// </summary>
    private async Task AnalyzeKeyVaultSecretsImpactAsync(
        AzureResource sourceResource,
        RoleAssignment roleAssignment,
        AzureResource targetResource,
        string sourceName,
        string sourceType,
        string targetName,
        string targetType,
        List<RoleAssignmentImpactItem> impacts,
        CancellationToken cancellationToken)
    {
        if (roleAssignment.RoleDefinitionId != AzureRoleDefinitionCatalog.KeyVaultSecretsUser)
            return;

        if (targetResource is not Domain.KeyVaultAggregate.KeyVault)
            return;

        // Check AppSettings on compute resources
        var kvSettingsCount = sourceResource.AppSettings
            .Count(s => s.KeyVaultResourceId == roleAssignment.TargetResourceId);

        if (kvSettingsCount > 0)
        {
            impacts.Add(new RoleAssignmentImpactItem(
                ImpactType: "KeyVaultSecretsRequired",
                AffectedResourceId: sourceResource.Id.Value,
                AffectedResourceName: sourceName,
                AffectedResourceType: sourceType,
                TargetResourceId: targetResource.Id.Value,
                TargetResourceName: targetName,
                TargetResourceType: targetType,
                Description: $"'{sourceName}' has {kvSettingsCount} app setting(s) referencing Key Vault '{targetName}'. Removing Key Vault Secrets User will break secret resolution.",
                Severity: "Critical",
                AffectedSettingsCount: kvSettingsCount));
        }

        // Check AppConfigurationKeys if the source is an AppConfiguration
        if (sourceResource is AppConfiguration)
        {
            var appConfig = await appConfigurationRepository.GetByIdWithConfigurationKeysAsync(
                sourceResource.Id, cancellationToken);

            if (appConfig is not null)
            {
                var kvKeysCount = appConfig.ConfigurationKeys
                    .Count(k => k.KeyVaultResourceId == roleAssignment.TargetResourceId);

                if (kvKeysCount > 0)
                {
                    impacts.Add(new RoleAssignmentImpactItem(
                        ImpactType: "KeyVaultSecretsRequired",
                        AffectedResourceId: sourceResource.Id.Value,
                        AffectedResourceName: sourceName,
                        AffectedResourceType: sourceType,
                        TargetResourceId: targetResource.Id.Value,
                        TargetResourceName: targetName,
                        TargetResourceType: targetType,
                        Description: $"'{sourceName}' has {kvKeysCount} configuration key(s) referencing Key Vault '{targetName}'. Removing Key Vault Secrets User will break Key Vault reference resolution.",
                        Severity: "Critical",
                        AffectedSettingsCount: kvKeysCount));
                }
            }
        }
    }

    /// <summary>
    /// Rule: If this is the last role assignment from the source to the target,
    /// the source will lose all access to the target resource.
    /// </summary>
    private static void AnalyzeLastRoleToTargetImpact(
        AzureResource sourceResource,
        RoleAssignment roleAssignment,
        string sourceName,
        string sourceType,
        string targetName,
        string targetType,
        List<RoleAssignmentImpactItem> impacts)
    {
        var rolesOnSameTarget = sourceResource.RoleAssignments
            .Count(r => r.TargetResourceId == roleAssignment.TargetResourceId);

        if (rolesOnSameTarget != 1)
            return;

        impacts.Add(new RoleAssignmentImpactItem(
            ImpactType: "LastRoleToTarget",
            AffectedResourceId: sourceResource.Id.Value,
            AffectedResourceName: sourceName,
            AffectedResourceType: sourceType,
            TargetResourceId: roleAssignment.TargetResourceId.Value,
            TargetResourceName: targetName,
            TargetResourceType: targetType,
            Description: $"This is the last role assignment from '{sourceName}' to '{targetName}'. The resource will lose all access.",
            Severity: "Warning"));
    }
}

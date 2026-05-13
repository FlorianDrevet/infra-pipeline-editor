using ErrorOr;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.AppConfigurations.Queries.ListAppConfigurationKeys;

/// <summary>Handles the <see cref="ListAppConfigurationKeysQuery"/> request.</summary>
public sealed class ListAppConfigurationKeysQueryHandler(
    IAppConfigurationRepository appConfigurationRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IProjectRepository projectRepository)
    : IQueryHandler<ListAppConfigurationKeysQuery, IReadOnlyList<AppConfigurationKeyResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<IReadOnlyList<AppConfigurationKeyResult>>> Handle(
        ListAppConfigurationKeysQuery request,
        CancellationToken cancellationToken)
    {
        var appConfig = await appConfigurationRepository.GetByIdWithConfigurationKeysAndRoleAssignmentsAsync(
            request.AppConfigurationId, cancellationToken);

        if (appConfig is null)
            return Errors.AppConfigurationKey.AppConfigurationNotFound(request.AppConfigurationId);

        var vgNameLookup = await ResolveVariableGroupNamesAsync(appConfig, cancellationToken);
        var kvIdsWithAccess = BuildKeyVaultAccessSet(appConfig);

        return appConfig.ConfigurationKeys
            .Select(k => MapKey(k, kvIdsWithAccess, vgNameLookup))
            .ToList();
    }

    private async Task<Dictionary<ProjectPipelineVariableGroupId, string>> ResolveVariableGroupNamesAsync(
        Domain.AppConfigurationAggregate.AppConfiguration appConfig,
        CancellationToken cancellationToken)
    {
        var vgIds = appConfig.ConfigurationKeys
            .Where(k => k.VariableGroupId is not null)
            .Select(k => k.VariableGroupId!)
            .Distinct()
            .ToList();

        if (vgIds.Count == 0)
            return [];

        var resourceGroup = await resourceGroupRepository.GetByIdReadOnlyAsync(
            appConfig.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return [];

        var authResult = await accessService.VerifyReadAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return [];

        var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
            authResult.Value.ProjectId, cancellationToken);

        if (project is null)
            return [];

        return project.PipelineVariableGroups
            .Where(g => vgIds.Contains(g.Id))
            .ToDictionary(g => g.Id, g => g.GroupName);
    }

    private static HashSet<AzureResourceId> BuildKeyVaultAccessSet(
        Domain.AppConfigurationAggregate.AppConfiguration appConfig)
    {
        return appConfig.RoleAssignments
            .Where(ra => AzureRoleDefinitionCatalog.KeyVaultSecretsAccessRoles.Contains(ra.RoleDefinitionId))
            .Select(ra => ra.TargetResourceId)
            .ToHashSet();
    }

    private static AppConfigurationKeyResult MapKey(
        Domain.AppConfigurationAggregate.Entities.AppConfigurationKey k,
        HashSet<AzureResourceId> kvIdsWithAccess,
        Dictionary<ProjectPipelineVariableGroupId, string> vgNameLookup)
    {
        bool? hasKeyVaultAccess = k.IsKeyVaultReference && k.KeyVaultResourceId is not null
            ? kvIdsWithAccess.Contains(k.KeyVaultResourceId)
            : null;

        string? variableGroupName = k.VariableGroupId is not null
            && vgNameLookup.TryGetValue(k.VariableGroupId, out var vgName)
                ? vgName
                : null;

        return new AppConfigurationKeyResult(
            k.Id,
            k.AppConfigurationId,
            k.Key,
            k.Label,
            k.EnvironmentValues.Count > 0
                ? k.EnvironmentValues.ToDictionary(ev => ev.EnvironmentName, ev => ev.Value)
                : null,
            k.SourceResourceId,
            k.SourceOutputName,
            k.IsOutputReference,
            k.KeyVaultResourceId,
            k.SecretName,
            k.IsKeyVaultReference,
            hasKeyVaultAccess,
            k.SecretValueAssignment,
            k.VariableGroupId?.Value,
            k.PipelineVariableName,
            variableGroupName,
            k.IsViaVariableGroup);
    }
}

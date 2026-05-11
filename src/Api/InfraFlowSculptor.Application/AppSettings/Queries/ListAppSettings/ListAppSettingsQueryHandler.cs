using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.AppSettings.Queries.ListAppSettings;

/// <summary>Handles the <see cref="ListAppSettingsQuery"/> request.</summary>
public sealed class ListAppSettingsQueryHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IProjectRepository projectRepository)
    : IQueryHandler<ListAppSettingsQuery, IReadOnlyList<AppSettingResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<IReadOnlyList<AppSettingResult>>> Handle(
        ListAppSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdWithRoleAssignmentsAndAppSettingsAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.AppSetting.SourceResourceNotFound(request.ResourceId);

        var vgNameLookup = await ResolveVariableGroupNamesAsync(resource, cancellationToken);
        var kvIdsWithAccess = BuildKeyVaultAccessSet(resource);

        return resource.AppSettings
            .Select(s => MapAppSetting(s, kvIdsWithAccess, vgNameLookup))
            .ToList();
    }

    private async Task<Dictionary<ProjectPipelineVariableGroupId, string>> ResolveVariableGroupNamesAsync(
        AzureResource resource,
        CancellationToken cancellationToken)
    {
        var vgIds = resource.AppSettings
            .Where(s => s.VariableGroupId is not null)
            .Select(s => s.VariableGroupId!)
            .Distinct()
            .ToList();

        if (vgIds.Count == 0)
            return [];

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            resource.ResourceGroupId, cancellationToken);

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

    private static HashSet<AzureResourceId> BuildKeyVaultAccessSet(AzureResource resource)
    {
        return resource.RoleAssignments
            .Where(ra => AzureRoleDefinitionCatalog.KeyVaultSecretsAccessRoles.Contains(ra.RoleDefinitionId))
            .Select(ra => ra.TargetResourceId)
            .ToHashSet();
    }

    private static AppSettingResult MapAppSetting(
        Domain.Common.BaseModels.Entites.AppSetting s,
        HashSet<AzureResourceId> kvIdsWithAccess,
        Dictionary<ProjectPipelineVariableGroupId, string> vgNameLookup)
    {
        bool? hasKeyVaultAccess = s.IsKeyVaultReference && s.KeyVaultResourceId is not null
            ? kvIdsWithAccess.Contains(s.KeyVaultResourceId)
            : null;

        string? variableGroupName = s.VariableGroupId is not null
            && vgNameLookup.TryGetValue(s.VariableGroupId, out var vgName)
                ? vgName
                : null;

        return new AppSettingResult(
            s.Id, s.ResourceId, s.Name,
            s.EnvironmentValues.Count > 0
                ? s.EnvironmentValues.ToDictionary(ev => ev.EnvironmentName, ev => ev.Value)
                : null,
            s.SourceResourceId,
            s.SourceOutputName, s.IsOutputReference,
            s.KeyVaultResourceId, s.SecretName,
            s.IsKeyVaultReference,
            hasKeyVaultAccess,
            s.SecretValueAssignment,
            s.VariableGroupId?.Value,
            s.PipelineVariableName,
            variableGroupName,
            s.IsViaVariableGroup);
    }
}

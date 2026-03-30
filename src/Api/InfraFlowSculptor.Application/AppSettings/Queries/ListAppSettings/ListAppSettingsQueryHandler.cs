using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
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
        var resource = await azureResourceRepository.GetByIdWithAppSettingsAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.AppSetting.SourceResourceNotFound(request.ResourceId);

        // Resolve VG group names if any app setting references a variable group
        var vgIds = resource.AppSettings
            .Where(s => s.VariableGroupId is not null)
            .Select(s => s.VariableGroupId!)
            .Distinct()
            .ToList();

        Dictionary<ProjectPipelineVariableGroupId, string> vgNameLookup = [];

        if (vgIds.Count > 0)
        {
            var resourceGroup = await resourceGroupRepository.GetByIdAsync(
                resource.ResourceGroupId, cancellationToken);

            if (resourceGroup is not null)
            {
                var authResult = await accessService.VerifyReadAccessAsync(
                    resourceGroup.InfraConfigId, cancellationToken);

                if (!authResult.IsError)
                {
                    var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
                        authResult.Value.ProjectId, cancellationToken);

                    if (project is not null)
                    {
                        vgNameLookup = project.PipelineVariableGroups
                            .Where(g => vgIds.Contains(g.Id))
                            .ToDictionary(g => g.Id, g => g.GroupName);
                    }
                }
            }
        }

        return resource.AppSettings
            .Select(s => new AppSettingResult(
                s.Id, s.ResourceId, s.Name,
                s.EnvironmentValues.Count > 0
                    ? s.EnvironmentValues.ToDictionary(ev => ev.EnvironmentName, ev => ev.Value)
                    : null,
                s.SourceResourceId,
                s.SourceOutputName, s.IsOutputReference,
                s.KeyVaultResourceId, s.SecretName,
                s.IsKeyVaultReference, null,
                s.SecretValueAssignment,
                s.VariableGroupId?.Value,
                s.PipelineVariableName,
                s.VariableGroupId is not null && vgNameLookup.TryGetValue(s.VariableGroupId, out var vgName) ? vgName : null,
                s.IsViaVariableGroup))
            .ToList();
    }
}

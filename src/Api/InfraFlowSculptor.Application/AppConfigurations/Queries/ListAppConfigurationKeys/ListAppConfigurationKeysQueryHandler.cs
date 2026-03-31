using ErrorOr;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
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
        var appConfig = await appConfigurationRepository.GetByIdWithConfigurationKeysAsync(
            request.AppConfigurationId, cancellationToken);

        if (appConfig is null)
            return Errors.AppConfigurationKey.AppConfigurationNotFound(request.AppConfigurationId);

        // Resolve VG group names if any key references a variable group
        var vgIds = appConfig.ConfigurationKeys
            .Where(k => k.VariableGroupId is not null)
            .Select(k => k.VariableGroupId!)
            .Distinct()
            .ToList();

        Dictionary<ProjectPipelineVariableGroupId, string> vgNameLookup = [];

        if (vgIds.Count > 0)
        {
            var resourceGroup = await resourceGroupRepository.GetByIdAsync(
                appConfig.ResourceGroupId, cancellationToken);

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

        return appConfig.ConfigurationKeys
            .Select(k => new AppConfigurationKeyResult(
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
                null,
                k.SecretValueAssignment,
                k.VariableGroupId?.Value,
                k.PipelineVariableName,
                k.VariableGroupId is not null && vgNameLookup.TryGetValue(k.VariableGroupId, out var vgName)
                    ? vgName
                    : null,
                k.IsViaVariableGroup))
            .ToList();
    }
}

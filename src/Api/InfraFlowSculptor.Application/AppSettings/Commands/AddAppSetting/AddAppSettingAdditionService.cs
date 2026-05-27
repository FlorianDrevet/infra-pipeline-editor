using ErrorOr;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ResourceOutputs;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;

namespace InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;

/// <summary>
/// Encapsulates the app-setting-specific mutation workflows previously owned by the command handler.
/// </summary>
public sealed class AddAppSettingAdditionService(
    IAzureResourceRepository azureResourceRepository,
    IProjectRepository projectRepository)
    : IAddAppSettingAdditionService
{
    /// <inheritdoc />
    public Task<ErrorOr<AppSettingResult>> AddAsync(
        AddAppSettingCommand request,
        AzureResource resource,
        DomainInfrastructureConfig infraConfig,
        CancellationToken cancellationToken)
    {
        if (ConfigurationValueSourceRequestPredicates.IsVariableGroupKeyVaultReference(request))
        {
            return AddVariableGroupKeyVaultReferenceAsync(
                request,
                resource,
                infraConfig,
                cancellationToken);
        }

        if (ConfigurationValueSourceRequestPredicates.IsVariableGroupReference(request))
            return AddVariableGroupReferenceAsync(request, resource, infraConfig, cancellationToken);

        if (ConfigurationValueSourceRequestPredicates.IsExportToKeyVault(request))
            return AddSensitiveOutputKeyVaultReferenceAsync(request, resource, cancellationToken);

        if (ConfigurationValueSourceRequestPredicates.IsKeyVaultReference(request))
            return AddKeyVaultReferenceAsync(request, resource, cancellationToken);

        if (ConfigurationValueSourceRequestPredicates.IsOutputReference(request))
            return AddOutputReferenceAsync(request, resource, cancellationToken);

        return AddStaticAsync(request, resource, cancellationToken);
    }

    private async Task<ErrorOr<AppSettingResult>> AddVariableGroupKeyVaultReferenceAsync(
        AddAppSettingCommand request,
        AzureResource resource,
        DomainInfrastructureConfig infraConfig,
        CancellationToken cancellationToken)
    {
        var variableGroupLookup = await ResolveVariableGroupAsync(
                infraConfig,
                request.VariableGroupId!.Value,
                cancellationToken)
            .ConfigureAwait(false);
        if (variableGroupLookup.IsError)
            return variableGroupLookup.Errors;

        var keyVaultLookup = await EnsureKeyVaultExistsAsync(
                request.KeyVaultResourceId!,
                cancellationToken)
            .ConfigureAwait(false);
        if (keyVaultLookup.IsError)
            return keyVaultLookup.Errors;

        var setting = resource.AddViaVariableGroupKeyVaultReferenceAppSetting(
            request.Name,
            variableGroupLookup.Value.Id,
            request.PipelineVariableName!,
            request.KeyVaultResourceId!,
            request.SecretName!,
            request.SecretValueAssignment ?? SecretValueAssignment.DirectInKeyVault);

        azureResourceRepository.Update(resource);

        var hasAccess = await CheckKeyVaultAccessAsync(
                request.ResourceId,
                request.KeyVaultResourceId!,
                cancellationToken)
            .ConfigureAwait(false);
        return ToResult(setting, hasAccess, variableGroupLookup.Value.GroupName);
    }

    private async Task<ErrorOr<AppSettingResult>> AddVariableGroupReferenceAsync(
        AddAppSettingCommand request,
        AzureResource resource,
        DomainInfrastructureConfig infraConfig,
        CancellationToken cancellationToken)
    {
        var variableGroupLookup = await ResolveVariableGroupAsync(
                infraConfig,
                request.VariableGroupId!.Value,
                cancellationToken)
            .ConfigureAwait(false);
        if (variableGroupLookup.IsError)
            return variableGroupLookup.Errors;

        var setting = resource.AddViaVariableGroupAppSetting(
            request.Name,
            variableGroupLookup.Value.Id,
            request.PipelineVariableName!);

        azureResourceRepository.Update(resource);
        return ToResult(setting, null, variableGroupLookup.Value.GroupName);
    }

    private async Task<ErrorOr<AppSettingResult>> AddSensitiveOutputKeyVaultReferenceAsync(
        AddAppSettingCommand request,
        AzureResource resource,
        CancellationToken cancellationToken)
    {
        var outputLookup = await EnsureValidOutputAsync(
                request.SourceResourceId!,
                request.SourceOutputName!,
                cancellationToken)
            .ConfigureAwait(false);
        if (outputLookup.IsError)
            return outputLookup.Errors;

        var keyVaultLookup = await EnsureKeyVaultExistsAsync(
                request.KeyVaultResourceId!,
                cancellationToken)
            .ConfigureAwait(false);
        if (keyVaultLookup.IsError)
            return keyVaultLookup.Errors;

        var setting = resource.AddSensitiveOutputKeyVaultReferenceAppSetting(
            request.Name,
            request.SourceResourceId!,
            request.SourceOutputName!,
            request.KeyVaultResourceId!,
            request.SecretName!);

        azureResourceRepository.Update(resource);

        var hasAccess = await CheckKeyVaultAccessAsync(
                request.ResourceId,
                request.KeyVaultResourceId!,
                cancellationToken)
            .ConfigureAwait(false);
        return ToResult(setting, hasAccess);
    }

    private async Task<ErrorOr<AppSettingResult>> AddKeyVaultReferenceAsync(
        AddAppSettingCommand request,
        AzureResource resource,
        CancellationToken cancellationToken)
    {
        var keyVaultLookup = await EnsureKeyVaultExistsAsync(
                request.KeyVaultResourceId!,
                cancellationToken)
            .ConfigureAwait(false);
        if (keyVaultLookup.IsError)
            return keyVaultLookup.Errors;

        var setting = resource.AddKeyVaultReferenceAppSetting(
            request.Name,
            request.KeyVaultResourceId!,
            request.SecretName!,
            request.SecretValueAssignment ?? SecretValueAssignment.DirectInKeyVault);

        azureResourceRepository.Update(resource);

        var hasAccess = await CheckKeyVaultAccessAsync(
                request.ResourceId,
                request.KeyVaultResourceId!,
                cancellationToken)
            .ConfigureAwait(false);
        return ToResult(setting, hasAccess);
    }

    private async Task<ErrorOr<AppSettingResult>> AddOutputReferenceAsync(
        AddAppSettingCommand request,
        AzureResource resource,
        CancellationToken cancellationToken)
    {
        var outputLookup = await EnsureValidOutputAsync(
                request.SourceResourceId!,
                request.SourceOutputName!,
                cancellationToken)
            .ConfigureAwait(false);
        if (outputLookup.IsError)
            return outputLookup.Errors;

        var setting = resource.AddOutputReferenceAppSetting(
            request.Name,
            request.SourceResourceId!,
            request.SourceOutputName!);

        azureResourceRepository.Update(resource);
        return ToResult(setting, null);
    }

    private async Task<ErrorOr<AppSettingResult>> AddStaticAsync(
        AddAppSettingCommand request,
        AzureResource resource,
        CancellationToken cancellationToken)
    {
        var setting = resource.AddStaticAppSetting(
            request.Name,
            request.EnvironmentValues ?? new Dictionary<string, string>());

        azureResourceRepository.Update(resource);
        return ToResult(setting, null);
    }

    private async Task<ErrorOr<ProjectPipelineVariableGroup>> ResolveVariableGroupAsync(
        DomainInfrastructureConfig infraConfig,
        Guid variableGroupIdValue,
        CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
                infraConfig.ProjectId,
                cancellationToken)
            .ConfigureAwait(false);

        var variableGroupId = new ProjectPipelineVariableGroupId(variableGroupIdValue);
        var variableGroup = project?.PipelineVariableGroups
            .FirstOrDefault(group => group.Id == variableGroupId);

        if (variableGroup is null)
            return Errors.Project.VariableGroupNotFoundError(variableGroupId);

        return variableGroup;
    }

    private async Task<ErrorOr<AzureResource>> EnsureKeyVaultExistsAsync(
        AzureResourceId keyVaultResourceId,
        CancellationToken cancellationToken)
    {
        var keyVaultResource = await azureResourceRepository.GetByIdAsync(
                keyVaultResourceId,
                cancellationToken)
            .ConfigureAwait(false);
        if (keyVaultResource is not KeyVault)
            return Errors.AppSetting.KeyVaultNotFound(keyVaultResourceId);

        return keyVaultResource;
    }

    private async Task<ErrorOr<bool>> EnsureValidOutputAsync(
        AzureResourceId sourceResourceId,
        string sourceOutputName,
        CancellationToken cancellationToken)
    {
        var sourceResource = await azureResourceRepository.GetByIdAsync(
                sourceResourceId,
                cancellationToken)
            .ConfigureAwait(false);
        if (sourceResource is null)
            return Errors.AppSetting.SourceResourceNotFound(sourceResourceId);

        var sourceType = sourceResource.GetType().Name;
        var outputDefinition = ResourceOutputCatalog.FindOutput(sourceType, sourceOutputName);
        if (outputDefinition is null)
            return Errors.AppSetting.InvalidOutput(sourceOutputName, sourceType);

        return true;
    }

    private async Task<bool> CheckKeyVaultAccessAsync(
        AzureResourceId computeResourceId,
        AzureResourceId keyVaultResourceId,
        CancellationToken cancellationToken)
    {
        var resourceWithRoles = await azureResourceRepository.GetByIdWithRoleAssignmentsAsync(
                computeResourceId,
                cancellationToken)
            .ConfigureAwait(false);
        if (resourceWithRoles is null)
            return false;

        return resourceWithRoles.RoleAssignments.Any(roleAssignment =>
            roleAssignment.TargetResourceId == keyVaultResourceId
            && roleAssignment.RoleDefinitionId == AzureRoleDefinitionCatalog.KeyVaultSecretsUser);
    }

    private static AppSettingResult ToResult(
        AppSetting setting,
        bool? hasKeyVaultAccess,
        string? variableGroupName = null)
    {
        return new AppSettingResult(
            setting.Id,
            setting.ResourceId,
            setting.Name,
            setting.EnvironmentValues.Count > 0
                ? setting.EnvironmentValues.ToDictionary(value => value.EnvironmentName, value => value.Value)
                : null,
            setting.SourceResourceId,
            setting.SourceOutputName,
            setting.IsOutputReference,
            setting.KeyVaultResourceId,
            setting.SecretName,
            setting.IsKeyVaultReference,
            hasKeyVaultAccess,
            setting.SecretValueAssignment,
            setting.VariableGroupId?.Value,
            setting.PipelineVariableName,
            variableGroupName,
            setting.IsViaVariableGroup);
    }
}

using ErrorOr;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ResourceOutputs;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.AddAppConfigurationKey;

/// <summary>
/// Encapsulates the App Configuration key mutation workflows previously owned by the command handler.
/// </summary>
public sealed class AddAppConfigurationKeyAdditionService(
    IAppConfigurationRepository appConfigurationRepository,
    IAzureResourceRepository azureResourceRepository,
    IProjectRepository projectRepository)
    : IAddAppConfigurationKeyAdditionService
{
    /// <inheritdoc />
    public Task<ErrorOr<AppConfigurationKeyResult>> AddAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfiguration,
        DomainInfrastructureConfig infraConfig,
        CancellationToken cancellationToken)
    {
        if (IsVariableGroupKeyVaultReference(request))
        {
            return AddVariableGroupKeyVaultReferenceAsync(
                request,
                appConfiguration,
                infraConfig,
                cancellationToken);
        }

        if (IsVariableGroupReference(request))
            return AddVariableGroupReferenceAsync(request, appConfiguration, infraConfig, cancellationToken);

        if (IsExportToKeyVault(request))
            return AddSensitiveOutputKeyVaultReferenceAsync(request, appConfiguration, cancellationToken);

        if (IsKeyVaultReference(request))
            return AddKeyVaultReferenceAsync(request, appConfiguration, cancellationToken);

        if (IsOutputReference(request))
            return AddOutputReferenceAsync(request, appConfiguration, cancellationToken);

        return AddStaticAsync(request, appConfiguration);
    }

    private static bool IsVariableGroupKeyVaultReference(AddAppConfigurationKeyCommand request) =>
        request.VariableGroupId is not null
        && request.PipelineVariableName is not null
        && request.KeyVaultResourceId is not null
        && request.SecretName is not null;

    private static bool IsVariableGroupReference(AddAppConfigurationKeyCommand request) =>
        request.VariableGroupId is not null
        && request.PipelineVariableName is not null;

    private static bool IsExportToKeyVault(AddAppConfigurationKeyCommand request) =>
        request.ExportToKeyVault
        && request.SourceResourceId is not null
        && request.SourceOutputName is not null
        && request.KeyVaultResourceId is not null
        && request.SecretName is not null;

    private static bool IsKeyVaultReference(AddAppConfigurationKeyCommand request) =>
        request.KeyVaultResourceId is not null
        && request.SecretName is not null;

    private static bool IsOutputReference(AddAppConfigurationKeyCommand request) =>
        request.SourceResourceId is not null
        && request.SourceOutputName is not null;

    private async Task<ErrorOr<AppConfigurationKeyResult>> AddVariableGroupKeyVaultReferenceAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfiguration,
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

        var configurationKey = appConfiguration.AddViaVariableGroupKeyVaultReferenceConfigurationKey(
            request.Key,
            request.Label,
            variableGroupLookup.Value.Id,
            request.PipelineVariableName!,
            request.KeyVaultResourceId!,
            request.SecretName!,
            request.SecretValueAssignment ?? SecretValueAssignment.DirectInKeyVault);

        await appConfigurationRepository.UpdateAsync(appConfiguration).ConfigureAwait(false);

        var hasAccess = await CheckKeyVaultAccessAsync(
                request.AppConfigurationId,
                request.KeyVaultResourceId!,
                cancellationToken)
            .ConfigureAwait(false);
        return ToResult(configurationKey, hasAccess, variableGroupLookup.Value.GroupName);
    }

    private async Task<ErrorOr<AppConfigurationKeyResult>> AddVariableGroupReferenceAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfiguration,
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

        var configurationKey = appConfiguration.AddViaVariableGroupConfigurationKey(
            request.Key,
            request.Label,
            variableGroupLookup.Value.Id,
            request.PipelineVariableName!);

        await appConfigurationRepository.UpdateAsync(appConfiguration).ConfigureAwait(false);
        return ToResult(configurationKey, null, variableGroupLookup.Value.GroupName);
    }

    private async Task<ErrorOr<AppConfigurationKeyResult>> AddSensitiveOutputKeyVaultReferenceAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfiguration,
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

        var configurationKey = appConfiguration.AddSensitiveOutputKeyVaultReferenceConfigurationKey(
            request.Key,
            request.Label,
            request.SourceResourceId!,
            request.SourceOutputName!,
            request.KeyVaultResourceId!,
            request.SecretName!);

        await appConfigurationRepository.UpdateAsync(appConfiguration).ConfigureAwait(false);

        var hasAccess = await CheckKeyVaultAccessAsync(
                request.AppConfigurationId,
                request.KeyVaultResourceId!,
                cancellationToken)
            .ConfigureAwait(false);
        return ToResult(configurationKey, hasAccess);
    }

    private async Task<ErrorOr<AppConfigurationKeyResult>> AddKeyVaultReferenceAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfiguration,
        CancellationToken cancellationToken)
    {
        var keyVaultLookup = await EnsureKeyVaultExistsAsync(
                request.KeyVaultResourceId!,
                cancellationToken)
            .ConfigureAwait(false);
        if (keyVaultLookup.IsError)
            return keyVaultLookup.Errors;

        var configurationKey = appConfiguration.AddKeyVaultReferenceConfigurationKey(
            request.Key,
            request.Label,
            request.KeyVaultResourceId!,
            request.SecretName!,
            request.SecretValueAssignment ?? SecretValueAssignment.DirectInKeyVault);

        await appConfigurationRepository.UpdateAsync(appConfiguration).ConfigureAwait(false);

        var hasAccess = await CheckKeyVaultAccessAsync(
                request.AppConfigurationId,
                request.KeyVaultResourceId!,
                cancellationToken)
            .ConfigureAwait(false);
        return ToResult(configurationKey, hasAccess);
    }

    private async Task<ErrorOr<AppConfigurationKeyResult>> AddOutputReferenceAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfiguration,
        CancellationToken cancellationToken)
    {
        var outputLookup = await EnsureValidOutputAsync(
                request.SourceResourceId!,
                request.SourceOutputName!,
                cancellationToken)
            .ConfigureAwait(false);
        if (outputLookup.IsError)
            return outputLookup.Errors;

        var configurationKey = appConfiguration.AddOutputReferenceConfigurationKey(
            request.Key,
            request.Label,
            request.SourceResourceId!,
            request.SourceOutputName!);

        await appConfigurationRepository.UpdateAsync(appConfiguration).ConfigureAwait(false);
        return ToResult(configurationKey, null);
    }

    private async Task<ErrorOr<AppConfigurationKeyResult>> AddStaticAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfiguration)
    {
        var configurationKey = appConfiguration.AddStaticConfigurationKey(
            request.Key,
            request.Label,
            request.EnvironmentValues ?? new Dictionary<string, string>());

        await appConfigurationRepository.UpdateAsync(appConfiguration).ConfigureAwait(false);
        return ToResult(configurationKey, null);
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
            return Errors.AppConfigurationKey.KeyVaultNotFound(keyVaultResourceId);

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
            return Errors.AppConfigurationKey.SourceResourceNotFound(sourceResourceId);

        var sourceType = sourceResource.GetType().Name;
        var outputDefinition = ResourceOutputCatalog.FindOutput(sourceType, sourceOutputName);
        if (outputDefinition is null)
            return Errors.AppConfigurationKey.InvalidOutput(sourceOutputName, sourceType);

        return true;
    }

    private async Task<bool> CheckKeyVaultAccessAsync(
        AzureResourceId appConfigurationResourceId,
        AzureResourceId keyVaultResourceId,
        CancellationToken cancellationToken)
    {
        var resourceWithRoles = await azureResourceRepository.GetByIdWithRoleAssignmentsAsync(
                appConfigurationResourceId,
                cancellationToken)
            .ConfigureAwait(false);
        if (resourceWithRoles is null)
            return false;

        return resourceWithRoles.RoleAssignments.Any(roleAssignment =>
            roleAssignment.TargetResourceId == keyVaultResourceId
            && roleAssignment.RoleDefinitionId == AzureRoleDefinitionCatalog.KeyVaultSecretsUser);
    }

    private static AppConfigurationKeyResult ToResult(
        AppConfigurationKey configurationKey,
        bool? hasKeyVaultAccess,
        string? variableGroupName = null)
    {
        return new AppConfigurationKeyResult(
            configurationKey.Id,
            configurationKey.AppConfigurationId,
            configurationKey.Key,
            configurationKey.Label,
            configurationKey.EnvironmentValues.Count > 0
                ? configurationKey.EnvironmentValues.ToDictionary(value => value.EnvironmentName, value => value.Value)
                : null,
            configurationKey.SourceResourceId,
            configurationKey.SourceOutputName,
            configurationKey.IsOutputReference,
            configurationKey.KeyVaultResourceId,
            configurationKey.SecretName,
            configurationKey.IsKeyVaultReference,
            hasKeyVaultAccess,
            configurationKey.SecretValueAssignment,
            configurationKey.VariableGroupId?.Value,
            configurationKey.PipelineVariableName,
            variableGroupName,
            configurationKey.IsViaVariableGroup);
    }
}
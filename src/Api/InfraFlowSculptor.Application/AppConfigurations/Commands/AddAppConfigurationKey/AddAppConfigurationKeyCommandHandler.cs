using ErrorOr;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ResourceOutputs;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.AddAppConfigurationKey;

/// <summary>Handles the <see cref="AddAppConfigurationKeyCommand"/> request.</summary>
public sealed class AddAppConfigurationKeyCommandHandler(
    IAppConfigurationRepository appConfigurationRepository,
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IProjectRepository projectRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<AddAppConfigurationKeyCommand, AppConfigurationKeyResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<AppConfigurationKeyResult>> Handle(
        AddAppConfigurationKeyCommand request,
        CancellationToken cancellationToken)
    {
        var appConfig = await appConfigurationRepository.GetByIdWithConfigurationKeysAsync(
            request.AppConfigurationId, cancellationToken);

        if (appConfig is null)
            return Errors.AppConfigurationKey.AppConfigurationNotFound(request.AppConfigurationId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            appConfig.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(appConfig.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        if (appConfig.ConfigurationKeys.Any(k =>
                string.Equals(k.Key, request.Key, StringComparison.OrdinalIgnoreCase)))
            return Errors.AppConfigurationKey.DuplicateKeyError(request.Key);

        return await DispatchAsync(request, appConfig, authResult.Value, cancellationToken);
    }

    private Task<ErrorOr<AppConfigurationKeyResult>> DispatchAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfig,
        Domain.InfrastructureConfigAggregate.InfrastructureConfig infraConfig,
        CancellationToken cancellationToken)
    {
        if (IsVariableGroupKeyVaultReference(request))
            return AddVariableGroupKeyVaultReferenceAsync(request, appConfig, infraConfig, cancellationToken);

        if (IsVariableGroupReference(request))
            return AddVariableGroupReferenceAsync(request, appConfig, infraConfig, cancellationToken);

        if (IsExportToKeyVault(request))
            return AddSensitiveOutputKeyVaultReferenceAsync(request, appConfig, cancellationToken);

        if (IsKeyVaultReference(request))
            return AddKeyVaultReferenceAsync(request, appConfig, cancellationToken);

        if (IsOutputReference(request))
            return AddOutputReferenceAsync(request, appConfig, cancellationToken);

        return AddStaticAsync(request, appConfig);
    }

    private static bool IsVariableGroupKeyVaultReference(AddAppConfigurationKeyCommand r) =>
        r.VariableGroupId is not null && r.PipelineVariableName is not null
        && r.KeyVaultResourceId is not null && r.SecretName is not null;

    private static bool IsVariableGroupReference(AddAppConfigurationKeyCommand r) =>
        r.VariableGroupId is not null && r.PipelineVariableName is not null;

    private static bool IsExportToKeyVault(AddAppConfigurationKeyCommand r) =>
        r.ExportToKeyVault
        && r.SourceResourceId is not null && r.SourceOutputName is not null
        && r.KeyVaultResourceId is not null && r.SecretName is not null;

    private static bool IsKeyVaultReference(AddAppConfigurationKeyCommand r) =>
        r.KeyVaultResourceId is not null && r.SecretName is not null;

    private static bool IsOutputReference(AddAppConfigurationKeyCommand r) =>
        r.SourceResourceId is not null && r.SourceOutputName is not null;

    private async Task<ErrorOr<AppConfigurationKeyResult>> AddVariableGroupKeyVaultReferenceAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfig,
        Domain.InfrastructureConfigAggregate.InfrastructureConfig infraConfig,
        CancellationToken cancellationToken)
    {
        var variableGroupLookup = await ResolveVariableGroupAsync(infraConfig, request.VariableGroupId!.Value, cancellationToken);
        if (variableGroupLookup.IsError) return variableGroupLookup.Errors;

        var keyVaultLookup = await EnsureKeyVaultExistsAsync(request.KeyVaultResourceId!, cancellationToken);
        if (keyVaultLookup.IsError) return keyVaultLookup.Errors;

        var configKey = appConfig.AddViaVariableGroupKeyVaultReferenceConfigurationKey(
            request.Key,
            request.Label,
            variableGroupLookup.Value.Id,
            request.PipelineVariableName!,
            request.KeyVaultResourceId!,
            request.SecretName!,
            request.SecretValueAssignment ?? SecretValueAssignment.DirectInKeyVault);

        await appConfigurationRepository.UpdateAsync(appConfig);

        var hasAccess = await CheckKeyVaultAccessAsync(request.AppConfigurationId, request.KeyVaultResourceId!, cancellationToken);
        return ToResult(configKey, hasAccess, variableGroupLookup.Value.GroupName);
    }

    private async Task<ErrorOr<AppConfigurationKeyResult>> AddVariableGroupReferenceAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfig,
        Domain.InfrastructureConfigAggregate.InfrastructureConfig infraConfig,
        CancellationToken cancellationToken)
    {
        var variableGroupLookup = await ResolveVariableGroupAsync(infraConfig, request.VariableGroupId!.Value, cancellationToken);
        if (variableGroupLookup.IsError) return variableGroupLookup.Errors;

        var configKey = appConfig.AddViaVariableGroupConfigurationKey(
            request.Key,
            request.Label,
            variableGroupLookup.Value.Id,
            request.PipelineVariableName!);

        await appConfigurationRepository.UpdateAsync(appConfig);
        return ToResult(configKey, null, variableGroupLookup.Value.GroupName);
    }

    private async Task<ErrorOr<AppConfigurationKeyResult>> AddSensitiveOutputKeyVaultReferenceAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfig,
        CancellationToken cancellationToken)
    {
        var outputLookup = await EnsureValidOutputAsync(request.SourceResourceId!, request.SourceOutputName!, cancellationToken);
        if (outputLookup.IsError) return outputLookup.Errors;

        var keyVaultLookup = await EnsureKeyVaultExistsAsync(request.KeyVaultResourceId!, cancellationToken);
        if (keyVaultLookup.IsError) return keyVaultLookup.Errors;

        var configKey = appConfig.AddSensitiveOutputKeyVaultReferenceConfigurationKey(
            request.Key,
            request.Label,
            request.SourceResourceId!,
            request.SourceOutputName!,
            request.KeyVaultResourceId!,
            request.SecretName!);

        await appConfigurationRepository.UpdateAsync(appConfig);

        var hasAccess = await CheckKeyVaultAccessAsync(request.AppConfigurationId, request.KeyVaultResourceId!, cancellationToken);
        return ToResult(configKey, hasAccess);
    }

    private async Task<ErrorOr<AppConfigurationKeyResult>> AddKeyVaultReferenceAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfig,
        CancellationToken cancellationToken)
    {
        var keyVaultLookup = await EnsureKeyVaultExistsAsync(request.KeyVaultResourceId!, cancellationToken);
        if (keyVaultLookup.IsError) return keyVaultLookup.Errors;

        var configKey = appConfig.AddKeyVaultReferenceConfigurationKey(
            request.Key,
            request.Label,
            request.KeyVaultResourceId!,
            request.SecretName!,
            request.SecretValueAssignment ?? SecretValueAssignment.DirectInKeyVault);

        await appConfigurationRepository.UpdateAsync(appConfig);

        var hasAccess = await CheckKeyVaultAccessAsync(request.AppConfigurationId, request.KeyVaultResourceId!, cancellationToken);
        return ToResult(configKey, hasAccess);
    }

    private async Task<ErrorOr<AppConfigurationKeyResult>> AddOutputReferenceAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfig,
        CancellationToken cancellationToken)
    {
        var outputLookup = await EnsureValidOutputAsync(request.SourceResourceId!, request.SourceOutputName!, cancellationToken);
        if (outputLookup.IsError) return outputLookup.Errors;

        var configKey = appConfig.AddOutputReferenceConfigurationKey(
            request.Key,
            request.Label,
            request.SourceResourceId!,
            request.SourceOutputName!);

        await appConfigurationRepository.UpdateAsync(appConfig);
        return ToResult(configKey, null);
    }

    private async Task<ErrorOr<AppConfigurationKeyResult>> AddStaticAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfig)
    {
        var configKey = appConfig.AddStaticConfigurationKey(
            request.Key,
            request.Label,
            request.EnvironmentValues ?? new Dictionary<string, string>());

        await appConfigurationRepository.UpdateAsync(appConfig);
        return ToResult(configKey, null);
    }

    private async Task<ErrorOr<Domain.ProjectAggregate.Entities.ProjectPipelineVariableGroup>> ResolveVariableGroupAsync(
        Domain.InfrastructureConfigAggregate.InfrastructureConfig infraConfig,
        Guid variableGroupIdValue,
        CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
            infraConfig.ProjectId, cancellationToken);

        var variableGroupId = new ProjectPipelineVariableGroupId(variableGroupIdValue);
        var variableGroup = project?.PipelineVariableGroups
            .FirstOrDefault(g => g.Id == variableGroupId);

        if (variableGroup is null)
            return Errors.Project.VariableGroupNotFoundError(variableGroupId);

        return variableGroup;
    }

    private async Task<ErrorOr<Domain.Common.BaseModels.AzureResource>> EnsureKeyVaultExistsAsync(
        AzureResourceId keyVaultResourceId,
        CancellationToken cancellationToken)
    {
        var keyVaultResource = await azureResourceRepository.GetByIdAsync(keyVaultResourceId, cancellationToken);
        if (keyVaultResource is not KeyVault)
            return Errors.AppConfigurationKey.KeyVaultNotFound(keyVaultResourceId);
        return keyVaultResource;
    }

    private async Task<ErrorOr<bool>> EnsureValidOutputAsync(
        AzureResourceId sourceResourceId,
        string sourceOutputName,
        CancellationToken cancellationToken)
    {
        var sourceResource = await azureResourceRepository.GetByIdAsync(sourceResourceId, cancellationToken);
        if (sourceResource is null)
            return Errors.AppConfigurationKey.SourceResourceNotFound(sourceResourceId);

        var sourceType = sourceResource.GetType().Name;
        var outputDef = ResourceOutputCatalog.FindOutput(sourceType, sourceOutputName);

        if (outputDef is null)
            return Errors.AppConfigurationKey.InvalidOutput(sourceOutputName, sourceType);

        return true;
    }

    private async Task<bool> CheckKeyVaultAccessAsync(
        AzureResourceId appConfigResourceId,
        AzureResourceId keyVaultResourceId,
        CancellationToken cancellationToken)
    {
        var resourceWithRoles = await azureResourceRepository.GetByIdWithRoleAssignmentsAsync(
            appConfigResourceId, cancellationToken);

        if (resourceWithRoles is null)
            return false;

        return resourceWithRoles.RoleAssignments.Any(ra =>
            ra.TargetResourceId == keyVaultResourceId &&
            ra.RoleDefinitionId == AzureRoleDefinitionCatalog.KeyVaultSecretsUser);
    }

    private static AppConfigurationKeyResult ToResult(
        Domain.AppConfigurationAggregate.Entities.AppConfigurationKey configKey,
        bool? hasKeyVaultAccess,
        string? variableGroupName = null)
        => new(
            configKey.Id,
            configKey.AppConfigurationId,
            configKey.Key,
            configKey.Label,
            configKey.EnvironmentValues.Count > 0
                ? configKey.EnvironmentValues.ToDictionary(ev => ev.EnvironmentName, ev => ev.Value)
                : null,
            configKey.SourceResourceId,
            configKey.SourceOutputName,
            configKey.IsOutputReference,
            configKey.KeyVaultResourceId,
            configKey.SecretName,
            configKey.IsKeyVaultReference,
            hasKeyVaultAccess,
            configKey.SecretValueAssignment,
            configKey.VariableGroupId?.Value,
            configKey.PipelineVariableName,
            variableGroupName,
            configKey.IsViaVariableGroup);
}

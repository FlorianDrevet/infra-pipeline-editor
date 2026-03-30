using ErrorOr;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
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

        // Variable group + Key Vault reference
        if (request.VariableGroupId is not null && request.PipelineVariableName is not null
            && request.KeyVaultResourceId is not null && request.SecretName is not null)
        {
            var infraConfig = authResult.Value;
            var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
                infraConfig.ProjectId, cancellationToken);

            var variableGroupId = new ProjectPipelineVariableGroupId(request.VariableGroupId.Value);
            var variableGroup = project?.PipelineVariableGroups
                .FirstOrDefault(g => g.Id == variableGroupId);

            if (variableGroup is null)
                return Errors.Project.VariableGroupNotFoundError(variableGroupId);

            var keyVaultResource = await azureResourceRepository.GetByIdAsync(
                request.KeyVaultResourceId, cancellationToken);

            if (keyVaultResource is null || keyVaultResource is not KeyVault)
                return Errors.AppConfigurationKey.KeyVaultNotFound(request.KeyVaultResourceId);

            var configKey = appConfig.AddViaVariableGroupKeyVaultReferenceConfigurationKey(
                request.Key,
                request.Label,
                variableGroupId,
                request.PipelineVariableName,
                request.KeyVaultResourceId,
                request.SecretName,
                request.SecretValueAssignment ?? SecretValueAssignment.DirectInKeyVault);

            await appConfigurationRepository.UpdateAsync(appConfig);

            var hasAccess = await CheckKeyVaultAccessAsync(
                request.AppConfigurationId, request.KeyVaultResourceId, cancellationToken);

            return ToResult(configKey, hasAccess, variableGroup.GroupName);
        }

        // Variable group reference
        if (request.VariableGroupId is not null && request.PipelineVariableName is not null)
        {
            var infraConfig = authResult.Value;
            var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
                infraConfig.ProjectId, cancellationToken);

            var variableGroupId = new ProjectPipelineVariableGroupId(request.VariableGroupId.Value);
            var variableGroup = project?.PipelineVariableGroups
                .FirstOrDefault(g => g.Id == variableGroupId);

            if (variableGroup is null)
                return Errors.Project.VariableGroupNotFoundError(variableGroupId);

            var configKey = appConfig.AddViaVariableGroupConfigurationKey(
                request.Key,
                request.Label,
                variableGroupId,
                request.PipelineVariableName);

            await appConfigurationRepository.UpdateAsync(appConfig);

            return ToResult(configKey, null, variableGroup.GroupName);
        }

        // Key Vault reference
        if (request.KeyVaultResourceId is not null && request.SecretName is not null)
        {
            var keyVaultResource = await azureResourceRepository.GetByIdAsync(
                request.KeyVaultResourceId, cancellationToken);

            if (keyVaultResource is null || keyVaultResource is not KeyVault)
                return Errors.AppConfigurationKey.KeyVaultNotFound(request.KeyVaultResourceId);

            var configKey = appConfig.AddKeyVaultReferenceConfigurationKey(
                request.Key,
                request.Label,
                request.KeyVaultResourceId,
                request.SecretName,
                request.SecretValueAssignment ?? SecretValueAssignment.DirectInKeyVault);

            await appConfigurationRepository.UpdateAsync(appConfig);

            var hasAccess = await CheckKeyVaultAccessAsync(
                request.AppConfigurationId, request.KeyVaultResourceId, cancellationToken);

            return ToResult(configKey, hasAccess);
        }

        // Static value
        var staticKey = appConfig.AddStaticConfigurationKey(
            request.Key,
            request.Label,
            request.EnvironmentValues ?? new Dictionary<string, string>());

        await appConfigurationRepository.UpdateAsync(appConfig);

        return ToResult(staticKey, null);
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

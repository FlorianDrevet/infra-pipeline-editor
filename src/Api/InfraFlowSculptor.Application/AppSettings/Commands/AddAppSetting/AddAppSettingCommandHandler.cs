using ErrorOr;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ResourceOutputs;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using MediatR;

namespace InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;

/// <summary>Handles the <see cref="AddAppSettingCommand"/> request.</summary>
public sealed class AddAppSettingCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IProjectRepository projectRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<AddAppSettingCommand, AppSettingResult>
{
    /// <summary>Resource types that support app settings.</summary>
    private static readonly HashSet<string> SupportedTypes =
    [
        nameof(WebApp), nameof(FunctionApp), nameof(ContainerApp)
    ];

    /// <inheritdoc />
    public async Task<ErrorOr<AppSettingResult>> Handle(
        AddAppSettingCommand request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdWithAppSettingsAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.AppSetting.SourceResourceNotFound(request.ResourceId);

        var resourceTypeName = resource.GetType().Name;
        if (!SupportedTypes.Contains(resourceTypeName))
            return Errors.AppSetting.NotSupportedForResourceType(resourceTypeName);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            resource.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(resource.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        // Variable group reference: validate the group exists on the project
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

            var setting = resource.AddViaVariableGroupAppSetting(
                request.Name,
                variableGroupId,
                request.PipelineVariableName);

            await azureResourceRepository.UpdateAsync(resource, cancellationToken);

            return ToResult(setting, null, variableGroup.GroupName);
        }

        // Export sensitive output to Key Vault: validate source output + KV, then create combined reference
        if (request.ExportToKeyVault
            && request.SourceResourceId is not null && request.SourceOutputName is not null
            && request.KeyVaultResourceId is not null && request.SecretName is not null)
        {
            var sourceResource = await azureResourceRepository.GetByIdAsync(
                request.SourceResourceId, cancellationToken);

            if (sourceResource is null)
                return Errors.AppSetting.SourceResourceNotFound(request.SourceResourceId);

            var sourceType = sourceResource.GetType().Name;
            var outputDef = ResourceOutputCatalog.FindOutput(sourceType, request.SourceOutputName);

            if (outputDef is null)
                return Errors.AppSetting.InvalidOutput(request.SourceOutputName, sourceType);

            var keyVaultResource = await azureResourceRepository.GetByIdAsync(
                request.KeyVaultResourceId, cancellationToken);

            if (keyVaultResource is null || keyVaultResource is not KeyVault)
                return Errors.AppSetting.KeyVaultNotFound(request.KeyVaultResourceId);

            var setting = resource.AddSensitiveOutputKeyVaultReferenceAppSetting(
                request.Name,
                request.SourceResourceId,
                request.SourceOutputName,
                request.KeyVaultResourceId,
                request.SecretName);

            await azureResourceRepository.UpdateAsync(resource, cancellationToken);

            var hasAccess = await CheckKeyVaultAccessAsync(
                request.ResourceId, request.KeyVaultResourceId, cancellationToken);

            return ToResult(setting, hasAccess);
        }

        // Key Vault reference: validate the KV resource exists and check access
        if (request.KeyVaultResourceId is not null && request.SecretName is not null)
        {
            var keyVaultResource = await azureResourceRepository.GetByIdAsync(
                request.KeyVaultResourceId, cancellationToken);

            if (keyVaultResource is null || keyVaultResource is not KeyVault)
                return Errors.AppSetting.KeyVaultNotFound(request.KeyVaultResourceId);

            var setting = resource.AddKeyVaultReferenceAppSetting(
                request.Name,
                request.KeyVaultResourceId,
                request.SecretName,
                request.SecretValueAssignment ?? SecretValueAssignment.DirectInKeyVault);

            await azureResourceRepository.UpdateAsync(resource, cancellationToken);

            var hasAccess = await CheckKeyVaultAccessAsync(
                request.ResourceId, request.KeyVaultResourceId, cancellationToken);

            return ToResult(setting, hasAccess);
        }

        // Output reference: validate the source resource and output
        if (request.SourceResourceId is not null && request.SourceOutputName is not null)
        {
            var sourceResource = await azureResourceRepository.GetByIdAsync(
                request.SourceResourceId, cancellationToken);

            if (sourceResource is null)
                return Errors.AppSetting.SourceResourceNotFound(request.SourceResourceId);

            var sourceType = sourceResource.GetType().Name;
            var outputDef = ResourceOutputCatalog.FindOutput(sourceType, request.SourceOutputName);

            if (outputDef is null)
                return Errors.AppSetting.InvalidOutput(request.SourceOutputName, sourceType);

            var setting = resource.AddOutputReferenceAppSetting(
                request.Name,
                request.SourceResourceId,
                request.SourceOutputName);

            await azureResourceRepository.UpdateAsync(resource, cancellationToken);

            return ToResult(setting, null);
        }

        // Static value
        var staticSetting = resource.AddStaticAppSetting(
            request.Name,
            request.EnvironmentValues ?? new Dictionary<string, string>());

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);

        return ToResult(staticSetting, null);
    }

    private async Task<bool> CheckKeyVaultAccessAsync(
        AzureResourceId computeResourceId,
        AzureResourceId keyVaultResourceId,
        CancellationToken cancellationToken)
    {
        var resourceWithRoles = await azureResourceRepository.GetByIdWithRoleAssignmentsAsync(
            computeResourceId, cancellationToken);

        if (resourceWithRoles is null)
            return false;

        return resourceWithRoles.RoleAssignments.Any(ra =>
            ra.TargetResourceId == keyVaultResourceId &&
            ra.RoleDefinitionId == AzureRoleDefinitionCatalog.KeyVaultSecretsUser);
    }

    private static AppSettingResult ToResult(
        Domain.Common.BaseModels.Entites.AppSetting setting,
        bool? hasKeyVaultAccess,
        string? variableGroupName = null)
        => new(
            setting.Id, setting.ResourceId, setting.Name,
            setting.EnvironmentValues.Count > 0
                ? setting.EnvironmentValues.ToDictionary(ev => ev.EnvironmentName, ev => ev.Value)
                : null,
            setting.SourceResourceId,
            setting.SourceOutputName, setting.IsOutputReference,
            setting.KeyVaultResourceId, setting.SecretName,
            setting.IsKeyVaultReference, hasKeyVaultAccess,
            setting.SecretValueAssignment,
            setting.VariableGroupId?.Value,
            setting.PipelineVariableName,
            variableGroupName,
            setting.IsViaVariableGroup);
}

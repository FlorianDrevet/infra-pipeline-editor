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

        if (resource.AppSettings.Any(s => string.Equals(s.Name, request.Name, StringComparison.OrdinalIgnoreCase)))
            return Errors.AppSetting.DuplicateNameError(request.Name);

        return await DispatchAsync(request, resource, authResult.Value, cancellationToken);
    }

    private Task<ErrorOr<AppSettingResult>> DispatchAsync(
        AddAppSettingCommand request,
        Domain.Common.BaseModels.AzureResource resource,
        Domain.InfrastructureConfigAggregate.InfrastructureConfig infraConfig,
        CancellationToken cancellationToken)
    {
        if (IsVariableGroupKeyVaultReference(request))
            return AddVariableGroupKeyVaultReferenceAsync(request, resource, infraConfig, cancellationToken);

        if (IsVariableGroupReference(request))
            return AddVariableGroupReferenceAsync(request, resource, infraConfig, cancellationToken);

        if (IsExportToKeyVault(request))
            return AddSensitiveOutputKeyVaultReferenceAsync(request, resource, cancellationToken);

        if (IsKeyVaultReference(request))
            return AddKeyVaultReferenceAsync(request, resource, cancellationToken);

        if (IsOutputReference(request))
            return AddOutputReferenceAsync(request, resource, cancellationToken);

        return AddStaticAsync(request, resource, cancellationToken);
    }

    private static bool IsVariableGroupKeyVaultReference(AddAppSettingCommand r) =>
        r.VariableGroupId is not null && r.PipelineVariableName is not null
        && r.KeyVaultResourceId is not null && r.SecretName is not null;

    private static bool IsVariableGroupReference(AddAppSettingCommand r) =>
        r.VariableGroupId is not null && r.PipelineVariableName is not null;

    private static bool IsExportToKeyVault(AddAppSettingCommand r) =>
        r.ExportToKeyVault
        && r.SourceResourceId is not null && r.SourceOutputName is not null
        && r.KeyVaultResourceId is not null && r.SecretName is not null;

    private static bool IsKeyVaultReference(AddAppSettingCommand r) =>
        r.KeyVaultResourceId is not null && r.SecretName is not null;

    private static bool IsOutputReference(AddAppSettingCommand r) =>
        r.SourceResourceId is not null && r.SourceOutputName is not null;

    private async Task<ErrorOr<AppSettingResult>> AddVariableGroupKeyVaultReferenceAsync(
        AddAppSettingCommand request,
        Domain.Common.BaseModels.AzureResource resource,
        Domain.InfrastructureConfigAggregate.InfrastructureConfig infraConfig,
        CancellationToken cancellationToken)
    {
        var variableGroupLookup = await ResolveVariableGroupAsync(infraConfig, request.VariableGroupId!.Value, cancellationToken);
        if (variableGroupLookup.IsError) return variableGroupLookup.Errors;

        var keyVaultLookup = await EnsureKeyVaultExistsAsync(request.KeyVaultResourceId!, cancellationToken);
        if (keyVaultLookup.IsError) return keyVaultLookup.Errors;

        var setting = resource.AddViaVariableGroupKeyVaultReferenceAppSetting(
            request.Name,
            variableGroupLookup.Value.Id,
            request.PipelineVariableName!,
            request.KeyVaultResourceId!,
            request.SecretName!,
            request.SecretValueAssignment ?? SecretValueAssignment.DirectInKeyVault);

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);

        var hasAccess = await CheckKeyVaultAccessAsync(request.ResourceId, request.KeyVaultResourceId!, cancellationToken);
        return ToResult(setting, hasAccess, variableGroupLookup.Value.GroupName);
    }

    private async Task<ErrorOr<AppSettingResult>> AddVariableGroupReferenceAsync(
        AddAppSettingCommand request,
        Domain.Common.BaseModels.AzureResource resource,
        Domain.InfrastructureConfigAggregate.InfrastructureConfig infraConfig,
        CancellationToken cancellationToken)
    {
        var variableGroupLookup = await ResolveVariableGroupAsync(infraConfig, request.VariableGroupId!.Value, cancellationToken);
        if (variableGroupLookup.IsError) return variableGroupLookup.Errors;

        var setting = resource.AddViaVariableGroupAppSetting(
            request.Name,
            variableGroupLookup.Value.Id,
            request.PipelineVariableName!);

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);
        return ToResult(setting, null, variableGroupLookup.Value.GroupName);
    }

    private async Task<ErrorOr<AppSettingResult>> AddSensitiveOutputKeyVaultReferenceAsync(
        AddAppSettingCommand request,
        Domain.Common.BaseModels.AzureResource resource,
        CancellationToken cancellationToken)
    {
        var outputLookup = await EnsureValidOutputAsync(request.SourceResourceId!, request.SourceOutputName!, cancellationToken);
        if (outputLookup.IsError) return outputLookup.Errors;

        var keyVaultLookup = await EnsureKeyVaultExistsAsync(request.KeyVaultResourceId!, cancellationToken);
        if (keyVaultLookup.IsError) return keyVaultLookup.Errors;

        var setting = resource.AddSensitiveOutputKeyVaultReferenceAppSetting(
            request.Name,
            request.SourceResourceId!,
            request.SourceOutputName!,
            request.KeyVaultResourceId!,
            request.SecretName!);

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);

        var hasAccess = await CheckKeyVaultAccessAsync(request.ResourceId, request.KeyVaultResourceId!, cancellationToken);
        return ToResult(setting, hasAccess);
    }

    private async Task<ErrorOr<AppSettingResult>> AddKeyVaultReferenceAsync(
        AddAppSettingCommand request,
        Domain.Common.BaseModels.AzureResource resource,
        CancellationToken cancellationToken)
    {
        var keyVaultLookup = await EnsureKeyVaultExistsAsync(request.KeyVaultResourceId!, cancellationToken);
        if (keyVaultLookup.IsError) return keyVaultLookup.Errors;

        var setting = resource.AddKeyVaultReferenceAppSetting(
            request.Name,
            request.KeyVaultResourceId!,
            request.SecretName!,
            request.SecretValueAssignment ?? SecretValueAssignment.DirectInKeyVault);

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);

        var hasAccess = await CheckKeyVaultAccessAsync(request.ResourceId, request.KeyVaultResourceId!, cancellationToken);
        return ToResult(setting, hasAccess);
    }

    private async Task<ErrorOr<AppSettingResult>> AddOutputReferenceAsync(
        AddAppSettingCommand request,
        Domain.Common.BaseModels.AzureResource resource,
        CancellationToken cancellationToken)
    {
        var outputLookup = await EnsureValidOutputAsync(request.SourceResourceId!, request.SourceOutputName!, cancellationToken);
        if (outputLookup.IsError) return outputLookup.Errors;

        var setting = resource.AddOutputReferenceAppSetting(
            request.Name,
            request.SourceResourceId!,
            request.SourceOutputName!);

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);
        return ToResult(setting, null);
    }

    private async Task<ErrorOr<AppSettingResult>> AddStaticAsync(
        AddAppSettingCommand request,
        Domain.Common.BaseModels.AzureResource resource,
        CancellationToken cancellationToken)
    {
        var setting = resource.AddStaticAppSetting(
            request.Name,
            request.EnvironmentValues ?? new Dictionary<string, string>());

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);
        return ToResult(setting, null);
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
            return Errors.AppSetting.KeyVaultNotFound(keyVaultResourceId);
        return keyVaultResource;
    }

    private async Task<ErrorOr<bool>> EnsureValidOutputAsync(
        AzureResourceId sourceResourceId,
        string sourceOutputName,
        CancellationToken cancellationToken)
    {
        var sourceResource = await azureResourceRepository.GetByIdAsync(sourceResourceId, cancellationToken);
        if (sourceResource is null)
            return Errors.AppSetting.SourceResourceNotFound(sourceResourceId);

        var sourceType = sourceResource.GetType().Name;
        var outputDef = ResourceOutputCatalog.FindOutput(sourceType, sourceOutputName);

        if (outputDef is null)
            return Errors.AppSetting.InvalidOutput(sourceOutputName, sourceType);

        return true;
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

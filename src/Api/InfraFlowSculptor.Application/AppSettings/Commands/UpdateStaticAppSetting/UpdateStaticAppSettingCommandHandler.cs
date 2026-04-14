using ErrorOr;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate;

namespace InfraFlowSculptor.Application.AppSettings.Commands.UpdateStaticAppSetting;

/// <summary>Handles the <see cref="UpdateStaticAppSettingCommand"/> request.</summary>
public sealed class UpdateStaticAppSettingCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<UpdateStaticAppSettingCommand, AppSettingResult>
{
    /// <summary>Resource types that support app settings.</summary>
    private static readonly HashSet<string> SupportedTypes =
    [
        nameof(WebApp), nameof(FunctionApp), nameof(ContainerApp)
    ];

    /// <inheritdoc />
    public async Task<ErrorOr<AppSettingResult>> Handle(
        UpdateStaticAppSettingCommand request,
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

        var setting = resource.AppSettings.FirstOrDefault(s => s.Id == request.AppSettingId);
        if (setting is null)
            return Errors.AppSetting.NotFoundError(request.AppSettingId);

        if (!setting.IsStatic)
            return Errors.AppSetting.CannotEditNonStaticError();

        var hasDuplicate = resource.AppSettings.Any(s =>
            s.Id != request.AppSettingId
            && string.Equals(s.Name, request.Name, StringComparison.OrdinalIgnoreCase));

        if (hasDuplicate)
            return Errors.AppSetting.DuplicateNameError(request.Name);

        resource.UpdateAppSettingToStatic(request.AppSettingId, request.Name, request.EnvironmentValues);

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);

        return ToResult(setting);
    }

    private static AppSettingResult ToResult(Domain.Common.BaseModels.Entites.AppSetting setting)
        => new(
            setting.Id, setting.ResourceId, setting.Name,
            setting.EnvironmentValues.Count > 0
                ? setting.EnvironmentValues.ToDictionary(ev => ev.EnvironmentName, ev => ev.Value)
                : null,
            setting.SourceResourceId,
            setting.SourceOutputName, setting.IsOutputReference,
            setting.KeyVaultResourceId, setting.SecretName,
            setting.IsKeyVaultReference, null,
            setting.SecretValueAssignment,
            setting.VariableGroupId?.Value,
            setting.PipelineVariableName,
            null,
            setting.IsViaVariableGroup);
}

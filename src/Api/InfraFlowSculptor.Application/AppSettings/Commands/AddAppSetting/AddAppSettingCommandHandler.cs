using ErrorOr;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ResourceOutputs;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate;
using MediatR;

namespace InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;

/// <summary>Handles the <see cref="AddAppSettingCommand"/> request.</summary>
public sealed class AddAppSettingCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<AddAppSettingCommand, ErrorOr<AppSettingResult>>
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

            return new AppSettingResult(
                setting.Id, setting.ResourceId, setting.Name,
                setting.StaticValue, setting.SourceResourceId,
                setting.SourceOutputName, setting.IsOutputReference);
        }

        // Static value
        var staticSetting = resource.AddStaticAppSetting(
            request.Name,
            request.StaticValue ?? string.Empty);

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);

        return new AppSettingResult(
            staticSetting.Id, staticSetting.ResourceId, staticSetting.Name,
            staticSetting.StaticValue, staticSetting.SourceResourceId,
            staticSetting.SourceOutputName, staticSetting.IsOutputReference);
    }
}

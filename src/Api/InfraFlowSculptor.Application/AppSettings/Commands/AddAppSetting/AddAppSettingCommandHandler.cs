using ErrorOr;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate;

namespace InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;

/// <summary>Handles the <see cref="AddAppSettingCommand"/> request.</summary>
public sealed class AddAppSettingCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IAddAppSettingAdditionService additionService)
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

        if (resource.AppSettings.Any(setting =>
                string.Equals(setting.Name, request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return Errors.AppSetting.DuplicateNameError(request.Name);
        }

        return await additionService.AddAsync(request, resource, authResult.Value, cancellationToken)
            .ConfigureAwait(false);
    }
}

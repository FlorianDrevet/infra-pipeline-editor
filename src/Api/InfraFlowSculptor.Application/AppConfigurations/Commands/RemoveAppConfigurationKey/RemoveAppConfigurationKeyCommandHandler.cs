using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.RemoveAppConfigurationKey;

/// <summary>Handles the <see cref="RemoveAppConfigurationKeyCommand"/> request.</summary>
public sealed class RemoveAppConfigurationKeyCommandHandler(
    IAppConfigurationRepository appConfigurationRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<RemoveAppConfigurationKeyCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveAppConfigurationKeyCommand request,
        CancellationToken cancellationToken)
    {
        var appConfig = await appConfigurationRepository.GetByIdWithConfigurationKeysAsync(
            request.AppConfigurationId, cancellationToken);

        if (appConfig is null)
            return Errors.AppConfigurationKey.AppConfigurationNotFound(request.AppConfigurationId);

        var configKey = appConfig.ConfigurationKeys.FirstOrDefault(k => k.Id == request.AppConfigurationKeyId);
        if (configKey is null)
            return Errors.AppConfigurationKey.NotFoundError(request.AppConfigurationKeyId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            appConfig.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(appConfig.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        appConfig.RemoveConfigurationKey(request.AppConfigurationKeyId);
        await appConfigurationRepository.UpdateAsync(appConfig);

        return Result.Deleted;
    }
}

using ErrorOr;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.AddAppConfigurationKey;

/// <summary>Handles the <see cref="AddAppConfigurationKeyCommand"/> request.</summary>
public sealed class AddAppConfigurationKeyCommandHandler(
    IAppConfigurationRepository appConfigurationRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IAddAppConfigurationKeyAdditionService additionService)
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

        return await additionService.AddAsync(request, appConfig, authResult.Value, cancellationToken)
            .ConfigureAwait(false);
    }
}

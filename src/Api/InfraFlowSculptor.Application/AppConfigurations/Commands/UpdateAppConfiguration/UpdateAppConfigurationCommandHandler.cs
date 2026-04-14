using ErrorOr;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.UpdateAppConfiguration;

/// <summary>
/// Handles the <see cref="UpdateAppConfigurationCommand"/> request.
/// </summary>
public class UpdateAppConfigurationCommandHandler(
    IAppConfigurationRepository appConfigurationRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateAppConfigurationCommand, AppConfigurationResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<AppConfigurationResult>> Handle(
        UpdateAppConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        var appConfiguration = await appConfigurationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (appConfiguration is null)
            return Errors.AppConfiguration.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(appConfiguration.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.AppConfiguration.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        appConfiguration.Update(request.Name, request.Location);

        if (request.EnvironmentSettings is not null)
            appConfiguration.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName, ec.Sku, ec.SoftDeleteRetentionInDays, ec.PurgeProtectionEnabled, ec.DisableLocalAuth, ec.PublicNetworkAccess))
                    .ToList());

        var updatedAppConfiguration = await appConfigurationRepository.UpdateAsync(appConfiguration);

        return mapper.Map<AppConfigurationResult>(updatedAppConfiguration);
    }
}

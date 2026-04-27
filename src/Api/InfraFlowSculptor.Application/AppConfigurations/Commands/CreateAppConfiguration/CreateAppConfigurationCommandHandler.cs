using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.CreateAppConfiguration;

/// <summary>
/// Handles the <see cref="CreateAppConfigurationCommand"/> request.
/// </summary>
public class CreateAppConfigurationCommandHandler(
    IAppConfigurationRepository appConfigurationRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateAppConfigurationCommand, AppConfigurationResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<AppConfigurationResult>> Handle(
        CreateAppConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var appConfiguration = AppConfiguration.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName, ec.Sku, ec.SoftDeleteRetentionInDays, ec.PurgeProtectionEnabled, ec.DisableLocalAuth, ec.PublicNetworkAccess))
                .ToList(),
            isExisting: request.IsExisting);

        var savedAppConfiguration = await appConfigurationRepository.AddAsync(appConfiguration);

        return mapper.Map<AppConfigurationResult>(savedAppConfiguration);
    }
}

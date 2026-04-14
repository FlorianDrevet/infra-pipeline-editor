using ErrorOr;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.AppConfigurations.Queries;

/// <summary>
/// Handles the <see cref="GetAppConfigurationQuery"/> request
/// and returns the matching App Configuration if the caller is a member.
/// </summary>
public class GetAppConfigurationQueryHandler(
    IAppConfigurationRepository appConfigurationRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetAppConfigurationQuery, AppConfigurationResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<AppConfigurationResult>> Handle(
        GetAppConfigurationQuery query,
        CancellationToken cancellationToken)
    {
        var appConfiguration = await appConfigurationRepository.GetByIdAsync(query.Id, cancellationToken);
        if (appConfiguration is null)
            return Errors.AppConfiguration.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(appConfiguration.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.AppConfiguration.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return Errors.AppConfiguration.NotFoundError(query.Id);

        return mapper.Map<AppConfigurationResult>(appConfiguration);
    }
}

using ErrorOr;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.AppConfigurations.Queries;

/// <summary>
/// Handles the <see cref="GetAppConfigurationQuery"/> request
/// and returns the matching App Configuration if the caller is a member.
/// </summary>
public class GetAppConfigurationQueryHandler(
    IAppConfigurationRepository appConfigurationRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetAppConfigurationQuery, AppConfigurationResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<AppConfigurationResult>> Handle(
        GetAppConfigurationQuery query,
        CancellationToken cancellationToken)
    {
        var appConfiguration = await appConfigurationRepository.GetByIdReadOnlyAsync(query.Id, cancellationToken);
        if (appConfiguration is null)
            return Errors.AppConfiguration.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(appConfiguration.ResourceGroup!.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return Errors.AppConfiguration.NotFoundError(query.Id);

        return mapper.Map<AppConfigurationResult>(appConfiguration);
    }
}

using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.WebApps.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.WebApps.Queries;

/// <summary>Handles the <see cref="GetWebAppQuery"/> request.</summary>
public class GetWebAppQueryHandler(
    IWebAppRepository webAppRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetWebAppQuery, WebAppResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<WebAppResult>> Handle(
        GetWebAppQuery query,
        CancellationToken cancellationToken)
    {
        var webApp = await webAppRepository.GetByIdAsync(query.Id, cancellationToken);
        if (webApp is null)
            return Errors.WebApp.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(webApp.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.WebApp.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.WebApp.NotFoundError(query.Id);

        return mapper.Map<WebAppResult>(webApp);
    }
}

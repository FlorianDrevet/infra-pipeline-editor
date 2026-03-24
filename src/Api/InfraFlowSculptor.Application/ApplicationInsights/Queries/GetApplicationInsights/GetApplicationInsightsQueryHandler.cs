using ErrorOr;
using InfraFlowSculptor.Application.ApplicationInsights.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.ApplicationInsights.Queries.GetApplicationInsights;

/// <summary>
/// Handles the <see cref="GetApplicationInsightsQuery"/> request
/// and returns the matching Application Insights resource if the caller is a member.
/// </summary>
public sealed class GetApplicationInsightsQueryHandler(
    IApplicationInsightsRepository applicationInsightsRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<GetApplicationInsightsQuery, ErrorOr<ApplicationInsightsResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ApplicationInsightsResult>> Handle(
        GetApplicationInsightsQuery query,
        CancellationToken cancellationToken)
    {
        var applicationInsights = await applicationInsightsRepository.GetByIdAsync(query.Id, cancellationToken);
        if (applicationInsights is null)
            return Errors.ApplicationInsights.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(applicationInsights.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ApplicationInsights.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.ApplicationInsights.NotFoundError(query.Id);

        return mapper.Map<ApplicationInsightsResult>(applicationInsights);
    }
}

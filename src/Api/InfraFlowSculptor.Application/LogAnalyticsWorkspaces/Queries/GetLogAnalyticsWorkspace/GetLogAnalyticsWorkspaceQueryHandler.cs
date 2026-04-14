using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Queries.GetLogAnalyticsWorkspace;

/// <summary>
/// Handles the <see cref="GetLogAnalyticsWorkspaceQuery"/> request
/// and returns the matching Log Analytics Workspace if the caller is a member.
/// </summary>
public sealed class GetLogAnalyticsWorkspaceQueryHandler(
    ILogAnalyticsWorkspaceRepository logAnalyticsWorkspaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetLogAnalyticsWorkspaceQuery, LogAnalyticsWorkspaceResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<LogAnalyticsWorkspaceResult>> Handle(
        GetLogAnalyticsWorkspaceQuery query,
        CancellationToken cancellationToken)
    {
        var logAnalyticsWorkspace = await logAnalyticsWorkspaceRepository.GetByIdAsync(query.Id, cancellationToken);
        if (logAnalyticsWorkspace is null)
            return Errors.LogAnalyticsWorkspace.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(logAnalyticsWorkspace.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.LogAnalyticsWorkspace.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.LogAnalyticsWorkspace.NotFoundError(query.Id);

        return mapper.Map<LogAnalyticsWorkspaceResult>(logAnalyticsWorkspace);
    }
}

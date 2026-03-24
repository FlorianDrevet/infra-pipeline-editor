using ErrorOr;
using InfraFlowSculptor.Application.ApplicationInsights.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.ApplicationInsights.Commands.UpdateApplicationInsights;

/// <summary>
/// Handles the <see cref="UpdateApplicationInsightsCommand"/> request.
/// </summary>
public sealed class UpdateApplicationInsightsCommandHandler(
    IApplicationInsightsRepository applicationInsightsRepository,
    ILogAnalyticsWorkspaceRepository logAnalyticsWorkspaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<UpdateApplicationInsightsCommand, ErrorOr<ApplicationInsightsResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ApplicationInsightsResult>> Handle(
        UpdateApplicationInsightsCommand request,
        CancellationToken cancellationToken)
    {
        var applicationInsights = await applicationInsightsRepository.GetByIdAsync(request.Id, cancellationToken);
        if (applicationInsights is null)
            return Errors.ApplicationInsights.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(applicationInsights.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ApplicationInsights.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        // Verify the Log Analytics Workspace exists
        var logAnalyticsWorkspaceId = new AzureResourceId(request.LogAnalyticsWorkspaceId);
        var logAnalyticsWorkspace = await logAnalyticsWorkspaceRepository.GetByIdAsync(logAnalyticsWorkspaceId, cancellationToken);
        if (logAnalyticsWorkspace is null)
            return Errors.LogAnalyticsWorkspace.NotFoundError(logAnalyticsWorkspaceId);

        applicationInsights.Update(request.Name, request.Location, logAnalyticsWorkspaceId);

        if (request.EnvironmentSettings is not null)
            applicationInsights.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName, ec.SamplingPercentage, ec.RetentionInDays, ec.DisableIpMasking, ec.DisableLocalAuth, ec.IngestionMode))
                    .ToList());

        var updated = await applicationInsightsRepository.UpdateAsync(applicationInsights);

        return mapper.Map<ApplicationInsightsResult>(updated);
    }
}

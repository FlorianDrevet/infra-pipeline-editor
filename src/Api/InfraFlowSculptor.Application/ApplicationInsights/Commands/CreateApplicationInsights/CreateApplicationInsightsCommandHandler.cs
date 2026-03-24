using InfraFlowSculptor.Application.ApplicationInsights.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.ApplicationInsights.Commands.CreateApplicationInsights;

/// <summary>Handles the <see cref="CreateApplicationInsightsCommand"/> request.</summary>
public sealed class CreateApplicationInsightsCommandHandler(
    IApplicationInsightsRepository applicationInsightsRepository,
    ILogAnalyticsWorkspaceRepository logAnalyticsWorkspaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<CreateApplicationInsightsCommand, ErrorOr<ApplicationInsightsResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ApplicationInsightsResult>> Handle(
        CreateApplicationInsightsCommand request,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        // Verify the Log Analytics Workspace exists
        var logAnalyticsWorkspaceId = new AzureResourceId(request.LogAnalyticsWorkspaceId);
        var logAnalyticsWorkspace = await logAnalyticsWorkspaceRepository.GetByIdAsync(logAnalyticsWorkspaceId, cancellationToken);
        if (logAnalyticsWorkspace is null)
            return Errors.LogAnalyticsWorkspace.NotFoundError(logAnalyticsWorkspaceId);

        var applicationInsights = Domain.ApplicationInsightsAggregate.ApplicationInsights.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            logAnalyticsWorkspaceId,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName, ec.SamplingPercentage, ec.RetentionInDays, ec.DisableIpMasking, ec.DisableLocalAuth, ec.IngestionMode))
                .ToList());

        var saved = await applicationInsightsRepository.AddAsync(applicationInsights);

        return mapper.Map<ApplicationInsightsResult>(saved);
    }
}

using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.UpdateLogAnalyticsWorkspace;

/// <summary>
/// Handles the <see cref="UpdateLogAnalyticsWorkspaceCommand"/> request.
/// </summary>
public sealed class UpdateLogAnalyticsWorkspaceCommandHandler(
    ILogAnalyticsWorkspaceRepository logAnalyticsWorkspaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateLogAnalyticsWorkspaceCommand, LogAnalyticsWorkspaceResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<LogAnalyticsWorkspaceResult>> Handle(
        UpdateLogAnalyticsWorkspaceCommand request,
        CancellationToken cancellationToken)
    {
        var logAnalyticsWorkspace = await logAnalyticsWorkspaceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (logAnalyticsWorkspace is null)
            return Errors.LogAnalyticsWorkspace.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(logAnalyticsWorkspace.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.LogAnalyticsWorkspace.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        logAnalyticsWorkspace.Update(request.Name, request.Location);

        if (request.EnvironmentSettings is not null)
            logAnalyticsWorkspace.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName, ec.Sku, ec.RetentionInDays, ec.DailyQuotaGb))
                    .ToList());

        var updated = await logAnalyticsWorkspaceRepository.UpdateAsync(logAnalyticsWorkspace);

        return mapper.Map<LogAnalyticsWorkspaceResult>(updated);
    }
}

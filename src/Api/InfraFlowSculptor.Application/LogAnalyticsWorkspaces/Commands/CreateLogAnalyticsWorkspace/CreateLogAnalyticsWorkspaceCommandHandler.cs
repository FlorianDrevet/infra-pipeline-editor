using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using MapsterMapper;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.CreateLogAnalyticsWorkspace;

/// <summary>
/// Handles the <see cref="CreateLogAnalyticsWorkspaceCommand"/> request.
/// </summary>
public sealed class CreateLogAnalyticsWorkspaceCommandHandler(
    ILogAnalyticsWorkspaceRepository logAnalyticsWorkspaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateLogAnalyticsWorkspaceCommand, LogAnalyticsWorkspaceResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<LogAnalyticsWorkspaceResult>> Handle(
        CreateLogAnalyticsWorkspaceCommand request,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var logAnalyticsWorkspace = LogAnalyticsWorkspace.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName, ec.Sku, ec.RetentionInDays, ec.DailyQuotaGb))
                .ToList());

        var saved = await logAnalyticsWorkspaceRepository.AddAsync(logAnalyticsWorkspace);

        return mapper.Map<LogAnalyticsWorkspaceResult>(saved);
    }
}

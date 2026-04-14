using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.UpdateEventHubNamespace;

/// <summary>Handles the <see cref="UpdateEventHubNamespaceCommand"/> request.</summary>
public class UpdateEventHubNamespaceCommandHandler(
    IEventHubNamespaceRepository eventHubNamespaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateEventHubNamespaceCommand, EventHubNamespaceResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<EventHubNamespaceResult>> Handle(
        UpdateEventHubNamespaceCommand request,
        CancellationToken cancellationToken)
    {
        var eh = await eventHubNamespaceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (eh is null)
            return Errors.EventHubNamespace.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(eh.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.EventHubNamespace.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        eh.Update(request.Name, request.Location);

        if (request.EnvironmentSettings is not null)
            eh.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName, ec.Sku, ec.Capacity, ec.ZoneRedundant, ec.DisableLocalAuth, ec.MinimumTlsVersion, ec.AutoInflateEnabled, ec.MaxThroughputUnits))
                    .ToList());

        var updated = await eventHubNamespaceRepository.UpdateAsync(eh);

        return mapper.Map<EventHubNamespaceResult>(updated);
    }
}

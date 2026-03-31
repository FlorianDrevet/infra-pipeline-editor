using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate;
using MapsterMapper;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.CreateEventHubNamespace;

/// <summary>Handles the <see cref="CreateEventHubNamespaceCommand"/> request.</summary>
public class CreateEventHubNamespaceCommandHandler(
    IEventHubNamespaceRepository eventHubNamespaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateEventHubNamespaceCommand, EventHubNamespaceResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<EventHubNamespaceResult>> Handle(
        CreateEventHubNamespaceCommand request,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var eventHubNamespace = EventHubNamespace.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName, ec.Sku, ec.Capacity, ec.ZoneRedundant, ec.DisableLocalAuth, ec.MinimumTlsVersion, ec.AutoInflateEnabled, ec.MaxThroughputUnits))
                .ToList());

        var saved = await eventHubNamespaceRepository.AddAsync(eventHubNamespace);

        return mapper.Map<EventHubNamespaceResult>(saved);
    }
}

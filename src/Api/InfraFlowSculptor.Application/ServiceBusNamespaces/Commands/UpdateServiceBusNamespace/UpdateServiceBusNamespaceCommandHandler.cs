using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.UpdateServiceBusNamespace;

/// <summary>
/// Handles the <see cref="UpdateServiceBusNamespaceCommand"/> request.
/// </summary>
public class UpdateServiceBusNamespaceCommandHandler(
    IServiceBusNamespaceRepository serviceBusNamespaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<UpdateServiceBusNamespaceCommand, ErrorOr<ServiceBusNamespaceResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ServiceBusNamespaceResult>> Handle(
        UpdateServiceBusNamespaceCommand request,
        CancellationToken cancellationToken)
    {
        var sb = await serviceBusNamespaceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sb is null)
            return Errors.ServiceBusNamespace.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(sb.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ServiceBusNamespace.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        sb.Update(request.Name, request.Location);

        if (request.EnvironmentSettings is not null)
            sb.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName, ec.Sku, ec.Capacity, ec.ZoneRedundant, ec.DisableLocalAuth, ec.MinimumTlsVersion))
                    .ToList());

        var updated = await serviceBusNamespaceRepository.UpdateAsync(sb);

        return mapper.Map<ServiceBusNamespaceResult>(updated);
    }
}

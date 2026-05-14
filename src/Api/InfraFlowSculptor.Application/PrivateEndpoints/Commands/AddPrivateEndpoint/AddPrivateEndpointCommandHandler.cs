using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PrivateEndpoints.Common;
using InfraFlowSculptor.Domain.Common.Constants;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.PrivateEndpoints.Commands.AddPrivateEndpoint;

/// <summary>Handler for adding a private endpoint to an Azure resource.</summary>
public sealed class AddPrivateEndpointCommandHandler(
    IAzureResourceRepository resourceRepository,
    IVirtualNetworkRepository virtualNetworkRepository,
    IPrivateDnsZoneRepository privateDnsZoneRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper) : ICommandHandler<AddPrivateEndpointCommand, PrivateEndpointConfigResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<PrivateEndpointConfigResult>> Handle(
        AddPrivateEndpointCommand request,
        CancellationToken cancellationToken)
    {
        var resource = await resourceRepository.GetByIdWithPrivateEndpointsAsync(request.ResourceId, cancellationToken);
        if (resource is null)
            return Errors.AzureResource.NotFound(request.ResourceId);

        var authResult = await accessService.VerifyWriteAccessAsync(
            resource.ResourceGroup!.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        if (PrivateEndpointGroupIdCatalog.GroupIdsByResourceType.TryGetValue(resource.ResourceType, out var validGroupIds)
            && !validGroupIds.Contains(request.GroupId))
            return Errors.AzureResource.InvalidGroupId(request.GroupId, resource.ResourceType);

        if (!await virtualNetworkRepository.SubnetExistsAsync(request.SubnetId, cancellationToken))
            return Errors.AzureResource.SubnetNotFound(request.SubnetId);

        if (request.PrivateDnsZoneId is not null)
        {
            var dnsZone = await privateDnsZoneRepository.GetByIdReadOnlyAsync(request.PrivateDnsZoneId, cancellationToken);
            if (dnsZone is null)
                return Errors.AzureResource.PrivateDnsZoneNotFound(request.PrivateDnsZoneId);
        }

        var config = resource.AddPrivateEndpoint(
            request.SubnetId,
            request.GroupId,
            request.AutoApproval,
            request.PrivateDnsZoneId,
            request.CustomNetworkInterfaceName);

        return mapper.Map<PrivateEndpointConfigResult>(config);
    }
}

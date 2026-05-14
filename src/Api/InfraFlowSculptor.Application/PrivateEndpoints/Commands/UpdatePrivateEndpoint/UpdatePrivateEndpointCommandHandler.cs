using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PrivateEndpoints.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.PrivateEndpoints.Commands.UpdatePrivateEndpoint;

/// <summary>Handler for updating a private endpoint configuration.</summary>
public sealed class UpdatePrivateEndpointCommandHandler(
    IAzureResourceRepository resourceRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper) : ICommandHandler<UpdatePrivateEndpointCommand, PrivateEndpointConfigResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<PrivateEndpointConfigResult>> Handle(
        UpdatePrivateEndpointCommand request,
        CancellationToken cancellationToken)
    {
        var resource = await resourceRepository.GetByIdWithPrivateEndpointsAsync(request.ResourceId, cancellationToken);
        if (resource is null)
            return Errors.AzureResource.NotFound(request.ResourceId);

        var authResult = await accessService.VerifyWriteAccessAsync(
            resource.ResourceGroup!.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        resource.UpdatePrivateEndpoint(
            request.ConfigId,
            request.SubnetId,
            request.GroupId,
            request.AutoApproval,
            request.PrivateDnsZoneId,
            request.CustomNetworkInterfaceName);

        var updated = resource.PrivateEndpointConfigs.First(c => c.Id == request.ConfigId);
        return mapper.Map<PrivateEndpointConfigResult>(updated);
    }
}

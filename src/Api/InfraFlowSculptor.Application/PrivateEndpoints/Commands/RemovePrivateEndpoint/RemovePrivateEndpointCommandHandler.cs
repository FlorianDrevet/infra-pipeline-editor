using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.PrivateEndpoints.Commands.RemovePrivateEndpoint;

/// <summary>Handler for removing a private endpoint configuration.</summary>
public sealed class RemovePrivateEndpointCommandHandler(
    IAzureResourceRepository resourceRepository,
    IInfraConfigAccessService accessService) : ICommandHandler<RemovePrivateEndpointCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemovePrivateEndpointCommand request,
        CancellationToken cancellationToken)
    {
        var resource = await resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);
        if (resource is null)
            return Errors.AzureResource.NotFound(request.ResourceId);

        var authResult = await accessService.VerifyWriteAccessAsync(
            resource.ResourceGroup!.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        resource.RemovePrivateEndpoint(request.ConfigId);
        return Result.Deleted;
    }
}

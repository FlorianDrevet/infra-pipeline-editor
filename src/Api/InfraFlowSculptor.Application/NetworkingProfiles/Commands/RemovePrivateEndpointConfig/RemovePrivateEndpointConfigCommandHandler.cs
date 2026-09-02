using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;

namespace InfraFlowSculptor.Application.NetworkingProfiles.Commands.RemovePrivateEndpointConfig;

/// <summary>
/// Removes the private endpoint configuration from an Azure resource,
/// reverting it to public access.
/// </summary>
public sealed class RemovePrivateEndpointConfigCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<RemovePrivateEndpointConfigCommand, Success>
{
    public async Task<ErrorOr<Success>> Handle(
        RemovePrivateEndpointConfigCommand request,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(request.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var resource = await azureResourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);
        if (resource is null)
            return Error.NotFound("AzureResource.NotFound",
                $"Resource '{request.ResourceId}' not found.");

        resource.DisablePrivateEndpoint();
        azureResourceRepository.Update(resource);

        return Result.Success;
    }
}

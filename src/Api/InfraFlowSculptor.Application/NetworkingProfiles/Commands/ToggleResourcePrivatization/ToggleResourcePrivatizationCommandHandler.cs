using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.NetworkingProfiles.Commands.ToggleResourcePrivatization;

/// <summary>Handles toggling privatization on/off for an Azure resource.</summary>
public sealed class ToggleResourcePrivatizationCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    INetworkingProfileRepository networkingProfileRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<ToggleResourcePrivatizationCommand, Success>
{
    public async Task<ErrorOr<Success>> Handle(
        ToggleResourcePrivatizationCommand request,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(request.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        // Verify a networking profile exists for this infra config
        var profile = await networkingProfileRepository.GetByInfraConfigIdAsync(
            request.InfraConfigId, cancellationToken);

        if (profile is null)
            return Errors.NetworkingProfile.NotFoundForInfraConfig(request.InfraConfigId);

        // Load the resource
        var resource = await azureResourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);
        if (resource is null)
            return Error.NotFound("AzureResource.NotFound",
                $"Resource '{request.ResourceId}' not found.");

        if (request.IsPrivatized)
            resource.Privatize();
        else
            resource.Deprivatize();

        azureResourceRepository.Update(resource);
        return Result.Success;
    }
}

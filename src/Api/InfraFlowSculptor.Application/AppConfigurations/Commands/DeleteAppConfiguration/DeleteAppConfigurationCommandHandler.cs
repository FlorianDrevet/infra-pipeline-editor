using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.DeleteAppConfiguration;

/// <summary>
/// Handles the <see cref="DeleteAppConfigurationCommand"/> request.
/// </summary>
public class DeleteAppConfigurationCommandHandler(
    IAppConfigurationRepository appConfigurationRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DeleteAppConfigurationCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteAppConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        var appConfiguration = await appConfigurationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (appConfiguration is null)
            return Errors.AppConfiguration.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(appConfiguration.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.AppConfiguration.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        await appConfigurationRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}

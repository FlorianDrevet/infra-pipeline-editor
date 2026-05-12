using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.Projects.Common;

internal sealed class ApplicationFolderNameResolver(
    IContainerAppRepository containerAppRepository,
    IWebAppRepository webAppRepository,
    IFunctionAppRepository functionAppRepository)
    : IApplicationFolderNameResolver
{
    public async Task<string> ResolveAsync(
        AzureResourceReadModel resource,
        CancellationToken cancellationToken = default)
    {
        var resourceId = new AzureResourceId(resource.Id);

        return resource.ResourceType switch
        {
            AzureResourceTypes.ArmTypes.ContainerAppType =>
                (await containerAppRepository.GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false))?.ApplicationName
                ?? resource.Name,
            AzureResourceTypes.ArmTypes.WebAppType =>
                (await webAppRepository.GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false))?.ApplicationName
                ?? resource.Name,
            AzureResourceTypes.ArmTypes.FunctionAppType =>
                (await functionAppRepository.GetByIdAsync(resourceId, cancellationToken).ConfigureAwait(false))?.ApplicationName
                ?? resource.Name,
            _ => resource.Name,
        };
    }
}
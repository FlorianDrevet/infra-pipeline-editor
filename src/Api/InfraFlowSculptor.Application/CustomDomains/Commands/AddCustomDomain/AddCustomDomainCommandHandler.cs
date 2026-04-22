using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.CustomDomains.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.CustomDomains.Commands.AddCustomDomain;

/// <summary>Handles the <see cref="AddCustomDomainCommand"/> request.</summary>
public sealed class AddCustomDomainCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<AddCustomDomainCommand, CustomDomainResult>
{
    private static readonly HashSet<string> SupportedResourceTypes =
    [
        AzureResourceTypes.ContainerApp,
        AzureResourceTypes.WebApp,
        AzureResourceTypes.FunctionApp,
    ];

    /// <inheritdoc />
    public async Task<ErrorOr<CustomDomainResult>> Handle(
        AddCustomDomainCommand request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdWithCustomDomainsAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.CustomDomain.ResourceNotFound(request.ResourceId);

        if (!SupportedResourceTypes.Contains(resource.ResourceType))
            return Errors.CustomDomain.NotSupportedForResourceType(resource.ResourceType);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            resource.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(resource.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var result = resource.AddCustomDomain(
            request.EnvironmentName,
            request.DomainName,
            request.BindingType);

        if (result.IsError)
            return result.Errors;

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);

        var cd = result.Value;
        return new CustomDomainResult(cd.Id, cd.ResourceId, cd.EnvironmentName, cd.DomainName, cd.BindingType);
    }
}

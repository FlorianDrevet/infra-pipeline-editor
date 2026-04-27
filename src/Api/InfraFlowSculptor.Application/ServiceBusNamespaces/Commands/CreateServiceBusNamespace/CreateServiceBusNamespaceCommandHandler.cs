using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.CreateServiceBusNamespace;

/// <summary>
/// Handles the <see cref="CreateServiceBusNamespaceCommand"/> request.
/// </summary>
public class CreateServiceBusNamespaceCommandHandler(
    IServiceBusNamespaceRepository serviceBusNamespaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateServiceBusNamespaceCommand, ServiceBusNamespaceResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ServiceBusNamespaceResult>> Handle(
        CreateServiceBusNamespaceCommand request,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var serviceBusNamespace = ServiceBusNamespace.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName, ec.Sku, ec.Capacity, ec.ZoneRedundant, ec.DisableLocalAuth, ec.MinimumTlsVersion))
                .ToList(),
            isExisting: request.IsExisting);

        var saved = await serviceBusNamespaceRepository.AddAsync(serviceBusNamespace);

        return mapper.Map<ServiceBusNamespaceResult>(saved);
    }
}

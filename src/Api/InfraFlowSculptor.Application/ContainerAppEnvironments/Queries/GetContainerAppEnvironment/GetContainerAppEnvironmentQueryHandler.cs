using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Queries.GetContainerAppEnvironment;

/// <summary>
/// Handles the <see cref="GetContainerAppEnvironmentQuery"/> request
/// and returns the matching Container App Environment if the caller is a member.
/// </summary>
public sealed class GetContainerAppEnvironmentQueryHandler(
    IContainerAppEnvironmentRepository containerAppEnvironmentRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<GetContainerAppEnvironmentQuery, ErrorOr<ContainerAppEnvironmentResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ContainerAppEnvironmentResult>> Handle(
        GetContainerAppEnvironmentQuery query,
        CancellationToken cancellationToken)
    {
        var containerAppEnvironment = await containerAppEnvironmentRepository.GetByIdAsync(query.Id, cancellationToken);
        if (containerAppEnvironment is null)
            return Errors.ContainerAppEnvironment.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(containerAppEnvironment.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ContainerAppEnvironment.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.ContainerAppEnvironment.NotFoundError(query.Id);

        return mapper.Map<ContainerAppEnvironmentResult>(containerAppEnvironment);
    }
}

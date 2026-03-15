using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using MapsterMapper;
using MediatR;
using ErrorOr;
using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Common;

namespace InfraFlowSculptor.Application.ResourceGroups.Commands.CreateResourceGroup;

public class CreateResourceGroupCommandHandler(
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<CreateResourceGroupCommand, ErrorOr<ResourceGroupResult>>
{
    public async Task<ErrorOr<ResourceGroupResult>> Handle(CreateResourceGroupCommand request, CancellationToken cancellationToken)
    {
        var authResult = await InfraConfigAccessHelper.VerifyWriteAccessAsync(
            infraConfigRepository, currentUser, request.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var resourceGroup = Domain.ResourceGroupAggregate.ResourceGroup.Create(request.Name, request.InfraConfigId, request.Location);

        var savedResourceGroup = await resourceGroupRepository.AddAsync(resourceGroup);

        return mapper.Map<ResourceGroupResult>(savedResourceGroup);
    }
}

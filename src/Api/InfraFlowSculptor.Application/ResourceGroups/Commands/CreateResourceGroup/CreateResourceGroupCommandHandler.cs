using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using MapsterMapper;
using MediatR;
using ErrorOr;
using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Common;

namespace InfraFlowSculptor.Application.ResourceGroups.Commands.CreateResourceGroup;

public class CreateResourceGroupCommandHandler(
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateResourceGroupCommand, ResourceGroupResult>
{
    public async Task<ErrorOr<ResourceGroupResult>> Handle(CreateResourceGroupCommand request, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(request.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var resourceGroup = Domain.ResourceGroupAggregate.ResourceGroup.Create(request.Name, request.InfraConfigId, request.Location);

        var savedResourceGroup = await resourceGroupRepository.AddAsync(resourceGroup);

        return mapper.Map<ResourceGroupResult>(savedResourceGroup);
    }
}

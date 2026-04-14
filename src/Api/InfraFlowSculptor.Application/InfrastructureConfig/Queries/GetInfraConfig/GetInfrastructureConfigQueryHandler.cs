using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetInfraConfig;

public class GetInfrastructureConfigQueryHandler(
    IInfraConfigAccessService accessService,
    IResourceGroupRepository resourceGroupRepository,
    IMapper mapper)
    : IQueryHandler<GetInfrastructureConfigQuery, GetInfrastructureConfigResult>
{
    public async Task<ErrorOr<GetInfrastructureConfigResult>> Handle(
        GetInfrastructureConfigQuery command, CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(command.Id, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var result = mapper.Map<GetInfrastructureConfigResult>(accessResult.Value);

        var counts = await resourceGroupRepository.GetResourceCountsByInfraConfigIdsAsync(
            [result.Id], cancellationToken);

        if (counts.TryGetValue(result.Id.Value, out var c))
            result = result with { ResourceGroupCount = c.ResourceGroupCount, ResourceCount = c.ResourceCount };

        return result;
    }
}

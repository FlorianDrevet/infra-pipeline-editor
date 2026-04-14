using ErrorOr;
using InfraFlowSculptor.Application.CosmosDbs.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.CosmosDbs.Queries;

/// <summary>
/// Handles the <see cref="GetCosmosDbQuery"/> request
/// and returns the matching Cosmos DB account if the caller is a member.
/// </summary>
public class GetCosmosDbQueryHandler(
    ICosmosDbRepository cosmosDbRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetCosmosDbQuery, CosmosDbResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<CosmosDbResult>> Handle(
        GetCosmosDbQuery query,
        CancellationToken cancellationToken)
    {
        var cosmosDb = await cosmosDbRepository.GetByIdAsync(query.Id, cancellationToken);
        if (cosmosDb is null)
            return Errors.CosmosDb.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(cosmosDb.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.CosmosDb.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return Errors.CosmosDb.NotFoundError(query.Id);

        return mapper.Map<CosmosDbResult>(cosmosDb);
    }
}

using ErrorOr;
using InfraFlowSculptor.Application.CosmosDbs.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.CosmosDbs.Commands.UpdateCosmosDb;

/// <summary>
/// Handles the <see cref="UpdateCosmosDbCommand"/> request.
/// </summary>
public class UpdateCosmosDbCommandHandler(
    ICosmosDbRepository cosmosDbRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateCosmosDbCommand, CosmosDbResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<CosmosDbResult>> Handle(
        UpdateCosmosDbCommand request,
        CancellationToken cancellationToken)
    {
        var cosmosDb = await cosmosDbRepository.GetByIdAsync(request.Id, cancellationToken);
        if (cosmosDb is null)
            return Errors.CosmosDb.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(cosmosDb.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.CosmosDb.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        cosmosDb.Update(request.Name, request.Location);

        if (request.EnvironmentSettings is not null)
            cosmosDb.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName, ec.DatabaseApiType, ec.ConsistencyLevel, ec.MaxStalenessPrefix, ec.MaxIntervalInSeconds, ec.EnableAutomaticFailover, ec.EnableMultipleWriteLocations, ec.BackupPolicyType, ec.EnableFreeTier))
                    .ToList());

        var updatedCosmosDb = await cosmosDbRepository.UpdateAsync(cosmosDb);

        return mapper.Map<CosmosDbResult>(updatedCosmosDb);
    }
}

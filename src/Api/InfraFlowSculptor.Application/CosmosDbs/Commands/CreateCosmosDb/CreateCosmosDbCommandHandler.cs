using InfraFlowSculptor.Application.CosmosDbs.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.CosmosDbAggregate;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.CosmosDbs.Commands.CreateCosmosDb;

/// <summary>
/// Handles the <see cref="CreateCosmosDbCommand"/> request.
/// </summary>
public class CreateCosmosDbCommandHandler(
    ICosmosDbRepository cosmosDbRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateCosmosDbCommand, CosmosDbResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<CosmosDbResult>> Handle(
        CreateCosmosDbCommand request,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var cosmosDb = CosmosDb.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName, ec.DatabaseApiType, ec.ConsistencyLevel, ec.MaxStalenessPrefix, ec.MaxIntervalInSeconds, ec.EnableAutomaticFailover, ec.EnableMultipleWriteLocations, ec.BackupPolicyType, ec.EnableFreeTier))
                .ToList());

        var savedCosmosDb = await cosmosDbRepository.AddAsync(cosmosDb);

        return mapper.Map<CosmosDbResult>(savedCosmosDb);
    }
}

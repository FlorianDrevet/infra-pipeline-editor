using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.CosmosDbs.Commands.DeleteCosmosDb;

/// <summary>
/// Handles the <see cref="DeleteCosmosDbCommand"/> request.
/// </summary>
public class DeleteCosmosDbCommandHandler(
    ICosmosDbRepository cosmosDbRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DeleteCosmosDbCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteCosmosDbCommand request,
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

        await cosmosDbRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}

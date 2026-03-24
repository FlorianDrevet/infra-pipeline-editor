using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.SqlServers.Commands.DeleteSqlServer;

/// <summary>Handles the <see cref="DeleteSqlServerCommand"/> request.</summary>
public class DeleteSqlServerCommandHandler(
    ISqlServerRepository sqlServerRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<DeleteSqlServerCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteSqlServerCommand request,
        CancellationToken cancellationToken)
    {
        var server = await sqlServerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (server is null)
            return Errors.SqlServer.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(server.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.SqlServer.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await sqlServerRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}

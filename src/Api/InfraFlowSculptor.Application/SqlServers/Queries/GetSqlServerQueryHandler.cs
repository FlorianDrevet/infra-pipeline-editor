using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SqlServers.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.SqlServers.Queries;

/// <summary>Handles the <see cref="GetSqlServerQuery"/> request.</summary>
public class GetSqlServerQueryHandler(
    ISqlServerRepository sqlServerRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetSqlServerQuery, SqlServerResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<SqlServerResult>> Handle(
        GetSqlServerQuery query,
        CancellationToken cancellationToken)
    {
        var server = await sqlServerRepository.GetByIdAsync(query.Id, cancellationToken);
        if (server is null)
            return Errors.SqlServer.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(server.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.SqlServer.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.SqlServer.NotFoundError(query.Id);

        return mapper.Map<SqlServerResult>(server);
    }
}

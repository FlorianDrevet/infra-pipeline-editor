using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SqlServers.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.SqlServers.Queries;

/// <summary>Handles the <see cref="GetSqlServerQuery"/> request.</summary>
public class GetSqlServerQueryHandler(
    ISqlServerRepository sqlServerRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetSqlServerQuery, SqlServerResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<SqlServerResult>> Handle(
        GetSqlServerQuery query,
        CancellationToken cancellationToken)
    {
        var server = await sqlServerRepository.GetByIdReadOnlyAsync(query.Id, cancellationToken);
        if (server is null)
            return Errors.SqlServer.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(server.ResourceGroup!.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.SqlServer.NotFoundError(query.Id);

        return mapper.Map<SqlServerResult>(server);
    }
}

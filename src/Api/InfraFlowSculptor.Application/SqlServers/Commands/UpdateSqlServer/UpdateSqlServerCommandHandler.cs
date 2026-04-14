using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SqlServers.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.SqlServerAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.SqlServers.Commands.UpdateSqlServer;

/// <summary>Handles the <see cref="UpdateSqlServerCommand"/> request.</summary>
public class UpdateSqlServerCommandHandler(
    ISqlServerRepository sqlServerRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateSqlServerCommand, SqlServerResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<SqlServerResult>> Handle(
        UpdateSqlServerCommand request,
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

        var version = new SqlServerVersion(
            Enum.Parse<SqlServerVersion.SqlServerVersionEnum>(request.Version));

        server.Update(request.Name, request.Location, version, request.AdministratorLogin);

        if (request.EnvironmentSettings is not null)
            server.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName, ec.MinimalTlsVersion))
                    .ToList());

        var updated = await sqlServerRepository.UpdateAsync(server);

        return mapper.Map<SqlServerResult>(updated);
    }
}

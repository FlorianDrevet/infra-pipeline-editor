using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SqlServers.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.SqlServerAggregate;
using InfraFlowSculptor.Domain.SqlServerAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.SqlServers.Commands.CreateSqlServer;

/// <summary>Handles the <see cref="CreateSqlServerCommand"/> request.</summary>
public class CreateSqlServerCommandHandler(
    ISqlServerRepository sqlServerRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<CreateSqlServerCommand, ErrorOr<SqlServerResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<SqlServerResult>> Handle(
        CreateSqlServerCommand request,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var version = new SqlServerVersion(
            Enum.Parse<SqlServerVersion.SqlServerVersionEnum>(request.Version));

        var server = SqlServer.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            version,
            request.AdministratorLogin,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName, ec.MinimalTlsVersion))
                .ToList());

        var saved = await sqlServerRepository.AddAsync(server);

        return mapper.Map<SqlServerResult>(saved);
    }
}

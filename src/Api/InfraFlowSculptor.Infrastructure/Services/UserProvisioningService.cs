using System.Data;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace InfraFlowSculptor.Infrastructure.Services;

/// <summary>
/// Provisions authenticated users atomically using PostgreSQL upsert semantics.
/// </summary>
public sealed class UserProvisioningService(ProjectDbContext dbContext) : IUserProvisioningService
{
    private const string ProvisionUserSql = """
                                            WITH inserted AS (
                                                INSERT INTO \"User\" (\"Id\", \"EntraId\", \"Name_FirstName\", \"Name_LastName\")
                                                VALUES (@id, @entraId, @firstName, @lastName)
                                                ON CONFLICT (\"EntraId\") DO NOTHING
                                                RETURNING \"Id\"
                                            )
                                            SELECT \"Id\" FROM inserted
                                            UNION ALL
                                            SELECT \"Id\" FROM \"User\" WHERE \"EntraId\" = @entraId
                                            LIMIT 1;
                                            """;

    /// <inheritdoc />
    public async Task<UserId> EnsureProvisionedAsync(
        EntraId entraId,
        Name name,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entraId);
        ArgumentNullException.ThrowIfNull(name);

        var createdUserId = Guid.NewGuid();
        var connectionAlreadyOpen = dbContext.Database.GetDbConnection().State == ConnectionState.Open;

        if (!connectionAlreadyOpen)
        {
            await dbContext.Database.OpenConnectionAsync(cancellationToken);
        }

        try
        {
            await using var command = dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = ProvisionUserSql;
            command.Transaction = dbContext.Database.CurrentTransaction?.GetDbTransaction();

            command.Parameters.Add(CreateParameter(command, "id", createdUserId, DbType.Guid));
            command.Parameters.Add(CreateParameter(command, "entraId", entraId.Value, DbType.Guid));
            command.Parameters.Add(CreateParameter(command, "firstName", name.FirstName, DbType.String));
            command.Parameters.Add(CreateParameter(command, "lastName", name.LastName, DbType.String));

            var result = await command.ExecuteScalarAsync(cancellationToken);

            return result switch
            {
                Guid provisionedUserId => new UserId(provisionedUserId),
                string provisionedUserIdText when Guid.TryParse(provisionedUserIdText, out var provisionedUserId)
                    => new UserId(provisionedUserId),
                _ => throw new InvalidOperationException("User provisioning did not return a persisted user identifier.")
            };
        }
        finally
        {
            if (!connectionAlreadyOpen)
            {
                await dbContext.Database.CloseConnectionAsync();
            }
        }
    }

    private static IDbDataParameter CreateParameter(IDbCommand command, string name, object value, DbType dbType)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        parameter.DbType = dbType;
        return parameter;
    }
}
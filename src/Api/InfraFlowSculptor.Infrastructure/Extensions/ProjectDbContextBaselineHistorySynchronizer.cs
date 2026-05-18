using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Extensions;

internal static class ProjectDbContextBaselineHistorySynchronizer
{
    private const string InitialCreateMigrationId = "20260517074017_InitialCreate";
    private const string ManagedIdentityAcrAuthMode = "ManagedIdentity";
    private const int RequiredMarkerTableCount = 4;
    private const string ExistingSchemaMarkerTablesSql = """
        SELECT COUNT(*)
        FROM information_schema.tables
        WHERE table_schema = current_schema()
          AND table_name IN ('Projects', 'InfrastructureConfigs', 'PersonalAccessTokens', 'User');
        """;
    private const string RepairLegacyComputeAcrPullIdentityColumnsSql = """
        ALTER TABLE IF EXISTS "ContainerApps" ADD COLUMN IF NOT EXISTS "AcrPullIdentityId" uuid;
        UPDATE "ContainerApps" AS computeResource
        SET "AcrPullIdentityId" = azureResource."AssignedUserAssignedIdentityId"
        FROM "AzureResource" AS azureResource
        WHERE azureResource."Id" = computeResource."Id"
            AND computeResource."AcrPullIdentityId" IS NULL
            AND computeResource."ContainerRegistryId" IS NOT NULL
            AND computeResource."AcrAuthMode" = @managedIdentityAcrAuthMode
            AND azureResource."AssignedUserAssignedIdentityId" IS NOT NULL;

        ALTER TABLE IF EXISTS "FunctionApps" ADD COLUMN IF NOT EXISTS "AcrPullIdentityId" uuid;
        UPDATE "FunctionApps" AS computeResource
        SET "AcrPullIdentityId" = azureResource."AssignedUserAssignedIdentityId"
        FROM "AzureResource" AS azureResource
        WHERE azureResource."Id" = computeResource."Id"
            AND computeResource."AcrPullIdentityId" IS NULL
            AND computeResource."ContainerRegistryId" IS NOT NULL
            AND computeResource."AcrAuthMode" = @managedIdentityAcrAuthMode
            AND azureResource."AssignedUserAssignedIdentityId" IS NOT NULL;

        ALTER TABLE IF EXISTS "WebApps" ADD COLUMN IF NOT EXISTS "AcrPullIdentityId" uuid;
        UPDATE "WebApps" AS computeResource
        SET "AcrPullIdentityId" = azureResource."AssignedUserAssignedIdentityId"
        FROM "AzureResource" AS azureResource
        WHERE azureResource."Id" = computeResource."Id"
            AND computeResource."AcrPullIdentityId" IS NULL
            AND computeResource."ContainerRegistryId" IS NOT NULL
            AND computeResource."AcrAuthMode" = @managedIdentityAcrAuthMode
            AND azureResource."AssignedUserAssignedIdentityId" IS NOT NULL;
        """;
    private const string RepairLegacyFrontDoorSchemaSql = """
        CREATE TABLE IF NOT EXISTS "FrontDoors" (
            "Id" uuid NOT NULL,
            "WafPolicyEnabled" boolean NOT NULL,
            CONSTRAINT "PK_FrontDoors" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_FrontDoors_AzureResource_Id"
                FOREIGN KEY ("Id") REFERENCES "AzureResource" ("Id") ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS "FrontDoorEnvironmentSettings" (
            "Id" uuid NOT NULL,
            "FrontDoorId" uuid NOT NULL,
            "EnvironmentName" character varying(100) NOT NULL,
            "Sku" text NOT NULL,
            CONSTRAINT "PK_FrontDoorEnvironmentSettings" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_FrontDoorEnvironmentSettings_FrontDoors_FrontDoorId"
                FOREIGN KEY ("FrontDoorId") REFERENCES "FrontDoors" ("Id") ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS "FrontDoorOrigins" (
            "Id" uuid NOT NULL,
            "FrontDoorId" uuid NOT NULL,
            "TargetResourceId" uuid NOT NULL,
            "HostName" character varying(260),
            "PrivateLinkEnabled" boolean NOT NULL,
            "Weight" integer NOT NULL,
            "Priority" integer NOT NULL,
            CONSTRAINT "PK_FrontDoorOrigins" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_FrontDoorOrigins_FrontDoors_FrontDoorId"
                FOREIGN KEY ("FrontDoorId") REFERENCES "FrontDoors" ("Id") ON DELETE CASCADE
        );

        CREATE UNIQUE INDEX IF NOT EXISTS "IX_FrontDoorEnvironmentSettings_FrontDoorId_EnvironmentName"
            ON "FrontDoorEnvironmentSettings" ("FrontDoorId", "EnvironmentName");

        CREATE INDEX IF NOT EXISTS "IX_FrontDoorOrigins_FrontDoorId"
            ON "FrontDoorOrigins" ("FrontDoorId");
        """;
    private const string CreateHistoryTableSql = """
        CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
            "MigrationId" character varying(150) NOT NULL,
            "ProductVersion" character varying(32) NOT NULL,
            CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
        );
        """;
    private const string ExistingBaselineMigrationSql = """
        SELECT COUNT(*)
        FROM "__EFMigrationsHistory"
        WHERE "MigrationId" = @migrationId;
        """;
    private const string InsertBaselineMigrationSql = """
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES (@migrationId, @productVersion);
        """;

    public static async Task<bool> SynchronizeAsync(ProjectDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync();
        }

        try
        {
            var existingMarkerTableCount = await ExecuteScalarAsync(connection, ExistingSchemaMarkerTablesSql);
            if (existingMarkerTableCount != RequiredMarkerTableCount)
            {
                return false;
            }

            await ExecuteNonQueryAsync(
                connection,
                RepairLegacyComputeAcrPullIdentityColumnsSql,
                CreateParameter(connection, "managedIdentityAcrAuthMode", ManagedIdentityAcrAuthMode));

            await ExecuteNonQueryAsync(connection, RepairLegacyFrontDoorSchemaSql);

            await ExecuteNonQueryAsync(connection, CreateHistoryTableSql);

            var existingBaselineMigrationCount = await ExecuteScalarAsync(
                connection,
                ExistingBaselineMigrationSql,
                CreateParameter(connection, "migrationId", InitialCreateMigrationId));

            if (existingBaselineMigrationCount > 0)
            {
                return false;
            }

            await ExecuteNonQueryAsync(
                connection,
                InsertBaselineMigrationSql,
                CreateParameter(connection, "migrationId", InitialCreateMigrationId),
                CreateParameter(connection, "productVersion", GetEntityFrameworkProductVersion()));

            return true;
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "The command text is selected from internal constant SQL strings only.")]
    private static async Task<int> ExecuteScalarAsync(DbConnection connection, string commandText, params DbParameter[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;

        foreach (var parameter in parameters)
        {
            command.Parameters.Add(parameter);
        }

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "The command text is selected from internal constant SQL strings only.")]
    private static async Task ExecuteNonQueryAsync(DbConnection connection, string commandText, params DbParameter[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;

        foreach (var parameter in parameters)
        {
            command.Parameters.Add(parameter);
        }

        await command.ExecuteNonQueryAsync();
    }

    private static DbParameter CreateParameter(DbConnection connection, string parameterName, object value)
    {
        var parameter = connection.CreateCommand().CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.Value = value;
        return parameter;
    }

    private static string GetEntityFrameworkProductVersion()
    {
        return typeof(DbContext).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            .Split('+')[0]
            ?? typeof(DbContext).Assembly.GetName().Version?.ToString(3)
            ?? "10.0.0";
    }
}
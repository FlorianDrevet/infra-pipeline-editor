using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceTypeAndEnvSettingsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResourceType",
                table: "AzureResource",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AzureResource_ResourceType",
                table: "AzureResource",
                column: "ResourceType");

            // ── Backfill ResourceType from TPT child tables ─────────────────
            migrationBuilder.Sql("""
                UPDATE "AzureResource" SET "ResourceType" = 'KeyVault'              WHERE "Id" IN (SELECT "Id" FROM "KeyVaults");
                UPDATE "AzureResource" SET "ResourceType" = 'RedisCache'            WHERE "Id" IN (SELECT "Id" FROM "RedisCaches");
                UPDATE "AzureResource" SET "ResourceType" = 'StorageAccount'        WHERE "Id" IN (SELECT "Id" FROM "StorageAccounts");
                UPDATE "AzureResource" SET "ResourceType" = 'AppServicePlan'        WHERE "Id" IN (SELECT "Id" FROM "AppServicePlans");
                UPDATE "AzureResource" SET "ResourceType" = 'WebApp'                WHERE "Id" IN (SELECT "Id" FROM "WebApps");
                UPDATE "AzureResource" SET "ResourceType" = 'FunctionApp'           WHERE "Id" IN (SELECT "Id" FROM "FunctionApps");
                UPDATE "AzureResource" SET "ResourceType" = 'UserAssignedIdentity'  WHERE "Id" IN (SELECT "Id" FROM "UserAssignedIdentities");
                UPDATE "AzureResource" SET "ResourceType" = 'AppConfiguration'      WHERE "Id" IN (SELECT "Id" FROM "AppConfigurations");
                UPDATE "AzureResource" SET "ResourceType" = 'ContainerAppEnvironment' WHERE "Id" IN (SELECT "Id" FROM "ContainerAppEnvironments");
                UPDATE "AzureResource" SET "ResourceType" = 'ContainerApp'          WHERE "Id" IN (SELECT "Id" FROM "ContainerApps");
                UPDATE "AzureResource" SET "ResourceType" = 'LogAnalyticsWorkspace' WHERE "Id" IN (SELECT "Id" FROM "LogAnalyticsWorkspaces");
                UPDATE "AzureResource" SET "ResourceType" = 'ApplicationInsights'   WHERE "Id" IN (SELECT "Id" FROM "ApplicationInsights");
                UPDATE "AzureResource" SET "ResourceType" = 'CosmosDb'              WHERE "Id" IN (SELECT "Id" FROM "CosmosDbAccounts");
                UPDATE "AzureResource" SET "ResourceType" = 'SqlServer'             WHERE "Id" IN (SELECT "Id" FROM "SqlServers");
                UPDATE "AzureResource" SET "ResourceType" = 'SqlDatabase'           WHERE "Id" IN (SELECT "Id" FROM "SqlDatabases");
                UPDATE "AzureResource" SET "ResourceType" = 'ServiceBusNamespace'   WHERE "Id" IN (SELECT "Id" FROM "ServiceBusNamespaces");
                UPDATE "AzureResource" SET "ResourceType" = 'ContainerRegistry'     WHERE "Id" IN (SELECT "Id" FROM "ContainerRegistries");
                UPDATE "AzureResource" SET "ResourceType" = 'EventHubNamespace'     WHERE "Id" IN (SELECT "Id" FROM "EventHubNamespaces");
                """);

            // ── Create the vw_ResourceEnvironmentEntries view ───────────────
            migrationBuilder.Sql("""
                CREATE OR REPLACE VIEW "vw_ResourceEnvironmentEntries" AS
                SELECT r."ResourceGroupId", es."KeyVaultId" AS "ResourceId", es."EnvironmentName"
                  FROM "KeyVaultEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."KeyVaultId"
                UNION ALL
                SELECT r."ResourceGroupId", es."RedisCacheId", es."EnvironmentName"
                  FROM "RedisCacheEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."RedisCacheId"
                UNION ALL
                SELECT r."ResourceGroupId", es."StorageAccountId", es."EnvironmentName"
                  FROM "StorageAccountEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."StorageAccountId"
                UNION ALL
                SELECT r."ResourceGroupId", es."AppServicePlanId", es."EnvironmentName"
                  FROM "AppServicePlanEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."AppServicePlanId"
                UNION ALL
                SELECT r."ResourceGroupId", es."WebAppId", es."EnvironmentName"
                  FROM "WebAppEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."WebAppId"
                UNION ALL
                SELECT r."ResourceGroupId", es."FunctionAppId", es."EnvironmentName"
                  FROM "FunctionAppEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."FunctionAppId"
                UNION ALL
                SELECT r."ResourceGroupId", es."AppConfigurationId", es."EnvironmentName"
                  FROM "AppConfigurationEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."AppConfigurationId"
                UNION ALL
                SELECT r."ResourceGroupId", es."ContainerAppEnvironmentId", es."EnvironmentName"
                  FROM "ContainerAppEnvironmentEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."ContainerAppEnvironmentId"
                UNION ALL
                SELECT r."ResourceGroupId", es."ContainerAppId", es."EnvironmentName"
                  FROM "ContainerAppEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."ContainerAppId"
                UNION ALL
                SELECT r."ResourceGroupId", es."LogAnalyticsWorkspaceId", es."EnvironmentName"
                  FROM "LogAnalyticsWorkspaceEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."LogAnalyticsWorkspaceId"
                UNION ALL
                SELECT r."ResourceGroupId", es."ApplicationInsightsId", es."EnvironmentName"
                  FROM "ApplicationInsightsEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."ApplicationInsightsId"
                UNION ALL
                SELECT r."ResourceGroupId", es."CosmosDbId", es."EnvironmentName"
                  FROM "CosmosDbEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."CosmosDbId"
                UNION ALL
                SELECT r."ResourceGroupId", es."SqlServerId", es."EnvironmentName"
                  FROM "SqlServerEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."SqlServerId"
                UNION ALL
                SELECT r."ResourceGroupId", es."SqlDatabaseId", es."EnvironmentName"
                  FROM "SqlDatabaseEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."SqlDatabaseId"
                UNION ALL
                SELECT r."ResourceGroupId", es."ServiceBusNamespaceId", es."EnvironmentName"
                  FROM "ServiceBusNamespaceEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."ServiceBusNamespaceId"
                UNION ALL
                SELECT r."ResourceGroupId", es."ContainerRegistryId", es."EnvironmentName"
                  FROM "ContainerRegistryEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."ContainerRegistryId"
                UNION ALL
                SELECT r."ResourceGroupId", es."EventHubNamespaceId", es."EnvironmentName"
                  FROM "EventHubNamespaceEnvironmentSettings" es
                  INNER JOIN "AzureResource" r ON r."Id" = es."EventHubNamespaceId";
                """);

            // ── Create the vw_ChildToParentLinks view ───────────────────────
            migrationBuilder.Sql("""
                CREATE OR REPLACE VIEW "vw_ChildToParentLinks" AS
                SELECT w."Id" AS "ChildResourceId", w."AppServicePlanId" AS "ParentResourceId", r."ResourceGroupId"
                  FROM "WebApps" w INNER JOIN "AzureResource" r ON r."Id" = w."Id"
                UNION ALL
                SELECT f."Id", f."AppServicePlanId", r."ResourceGroupId"
                  FROM "FunctionApps" f INNER JOIN "AzureResource" r ON r."Id" = f."Id"
                UNION ALL
                SELECT ca."Id", ca."ContainerAppEnvironmentId", r."ResourceGroupId"
                  FROM "ContainerApps" ca INNER JOIN "AzureResource" r ON r."Id" = ca."Id"
                UNION ALL
                SELECT sd."Id", sd."SqlServerId", r."ResourceGroupId"
                  FROM "SqlDatabases" sd INNER JOIN "AzureResource" r ON r."Id" = sd."Id"
                UNION ALL
                SELECT ai."Id", ai."LogAnalyticsWorkspaceId", r."ResourceGroupId"
                  FROM "ApplicationInsights" ai INNER JOIN "AzureResource" r ON r."Id" = ai."Id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP VIEW IF EXISTS "vw_ChildToParentLinks";""");
            migrationBuilder.Sql("""DROP VIEW IF EXISTS "vw_ResourceEnvironmentEntries";""");

            migrationBuilder.DropIndex(
                name: "IX_AzureResource_ResourceType",
                table: "AzureResource");

            migrationBuilder.DropColumn(
                name: "ResourceType",
                table: "AzureResource");
        }
    }
}

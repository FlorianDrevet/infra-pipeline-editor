using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP VIEW IF EXISTS "vw_ResourceEnvironmentEntries";""");

            migrationBuilder.AlterColumn<string>(
                name: "RuntimeVersion",
                table: "WebApps",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DockerImageName",
                table: "WebApps",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EnvironmentName",
                table: "WebAppEnvironmentSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DockerImageTag",
                table: "WebAppEnvironmentSettings",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "StorageTables",
                type: "character varying(63)",
                maxLength: 63,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "StorageQueues",
                type: "character varying(63)",
                maxLength: 63,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EnvironmentName",
                table: "SqlServerEnvironmentSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EnvironmentName",
                table: "SqlDatabaseEnvironmentSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RoleDefinitionId",
                table: "RoleAssignments",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RuntimeVersion",
                table: "FunctionApps",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DockerImageName",
                table: "FunctionApps",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EnvironmentName",
                table: "FunctionAppEnvironmentSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DockerImageTag",
                table: "FunctionAppEnvironmentSettings",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BlobContainers",
                type: "character varying(63)",
                maxLength: 63,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EnvironmentName",
                table: "AppServicePlanEnvironmentSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
                        migrationBuilder.Sql("""DROP VIEW IF EXISTS "vw_ResourceEnvironmentEntries";""");

            migrationBuilder.AlterColumn<string>(
                name: "RuntimeVersion",
                table: "WebApps",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "DockerImageName",
                table: "WebApps",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EnvironmentName",
                table: "WebAppEnvironmentSettings",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "DockerImageTag",
                table: "WebAppEnvironmentSettings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "StorageTables",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(63)",
                oldMaxLength: 63);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "StorageQueues",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(63)",
                oldMaxLength: 63);

            migrationBuilder.AlterColumn<string>(
                name: "EnvironmentName",
                table: "SqlServerEnvironmentSettings",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "EnvironmentName",
                table: "SqlDatabaseEnvironmentSettings",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "RoleDefinitionId",
                table: "RoleAssignments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(36)",
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<string>(
                name: "RuntimeVersion",
                table: "FunctionApps",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "DockerImageName",
                table: "FunctionApps",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EnvironmentName",
                table: "FunctionAppEnvironmentSettings",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "DockerImageTag",
                table: "FunctionAppEnvironmentSettings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BlobContainers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(63)",
                oldMaxLength: 63);

            migrationBuilder.AlterColumn<string>(
                name: "EnvironmentName",
                table: "AppServicePlanEnvironmentSettings",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

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
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropSqlViewsForLinqReplacement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP VIEW IF EXISTS "vw_ChildToParentLinks";""");
            migrationBuilder.Sql("""DROP VIEW IF EXISTS "vw_ResourceEnvironmentEntries";""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE VIEW "vw_ChildToParentLinks" AS
                SELECT w."Id" AS "ChildResourceId", w."AppServicePlanId" AS "ParentResourceId", r."ResourceGroupId"
                FROM "WebApp" w INNER JOIN "AzureResource" r ON w."Id" = r."Id"
                UNION ALL
                SELECT f."Id", f."AppServicePlanId", r."ResourceGroupId"
                FROM "FunctionApp" f INNER JOIN "AzureResource" r ON f."Id" = r."Id"
                UNION ALL
                SELECT ca."Id", ca."ContainerAppEnvironmentId", r."ResourceGroupId"
                FROM "ContainerApp" ca INNER JOIN "AzureResource" r ON ca."Id" = r."Id"
                UNION ALL
                SELECT sd."Id", sd."SqlServerId", r."ResourceGroupId"
                FROM "SqlDatabase" sd INNER JOIN "AzureResource" r ON sd."Id" = r."Id"
                UNION ALL
                SELECT ai."Id", ai."LogAnalyticsWorkspaceId", r."ResourceGroupId"
                FROM "ApplicationInsights" ai INNER JOIN "AzureResource" r ON ai."Id" = r."Id";
                """);

            migrationBuilder.Sql("""
                CREATE OR REPLACE VIEW "vw_ResourceEnvironmentEntries" AS
                SELECT r."ResourceGroupId", es."KeyVaultId" AS "ResourceId", es."EnvironmentName"
                FROM "KeyVaultEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."KeyVaultId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."RedisCacheId", es."EnvironmentName"
                FROM "RedisCacheEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."RedisCacheId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."StorageAccountId", es."EnvironmentName"
                FROM "StorageAccountEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."StorageAccountId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."AppServicePlanId", es."EnvironmentName"
                FROM "AppServicePlanEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."AppServicePlanId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."WebAppId", es."EnvironmentName"
                FROM "WebAppEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."WebAppId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."FunctionAppId", es."EnvironmentName"
                FROM "FunctionAppEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."FunctionAppId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."AppConfigurationId", es."EnvironmentName"
                FROM "AppConfigurationEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."AppConfigurationId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."ContainerAppEnvironmentId", es."EnvironmentName"
                FROM "ContainerAppEnvironmentEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."ContainerAppEnvironmentId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."ContainerAppId", es."EnvironmentName"
                FROM "ContainerAppEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."ContainerAppId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."LogAnalyticsWorkspaceId", es."EnvironmentName"
                FROM "LogAnalyticsWorkspaceEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."LogAnalyticsWorkspaceId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."ApplicationInsightsId", es."EnvironmentName"
                FROM "ApplicationInsightsEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."ApplicationInsightsId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."CosmosDbId", es."EnvironmentName"
                FROM "CosmosDbEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."CosmosDbId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."SqlServerId", es."EnvironmentName"
                FROM "SqlServerEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."SqlServerId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."SqlDatabaseId", es."EnvironmentName"
                FROM "SqlDatabaseEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."SqlDatabaseId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."ServiceBusNamespaceId", es."EnvironmentName"
                FROM "ServiceBusNamespaceEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."ServiceBusNamespaceId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."ContainerRegistryId", es."EnvironmentName"
                FROM "ContainerRegistryEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."ContainerRegistryId" = r."Id"
                UNION ALL
                SELECT r."ResourceGroupId", es."EventHubNamespaceId", es."EnvironmentName"
                FROM "EventHubNamespaceEnvironmentSettings" es INNER JOIN "AzureResource" r ON es."EventHubNamespaceId" = r."Id";
                """);
        }
    }
}

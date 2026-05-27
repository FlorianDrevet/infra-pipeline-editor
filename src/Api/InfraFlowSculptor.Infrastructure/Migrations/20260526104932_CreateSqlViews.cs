using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateSqlViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE VIEW "vw_ChildToParentLinks" AS
                SELECT w."Id" AS "ChildResourceId",
                       w."AppServicePlanId" AS "ParentResourceId",
                       ar."ResourceGroupId"
                FROM "WebApps" w
                INNER JOIN "AzureResource" ar ON ar."Id" = w."Id"
                UNION ALL
                SELECT f."Id" AS "ChildResourceId",
                       f."AppServicePlanId" AS "ParentResourceId",
                       ar."ResourceGroupId"
                FROM "FunctionApps" f
                INNER JOIN "AzureResource" ar ON ar."Id" = f."Id"
                UNION ALL
                SELECT ca."Id" AS "ChildResourceId",
                       ca."ContainerAppEnvironmentId" AS "ParentResourceId",
                       ar."ResourceGroupId"
                FROM "ContainerApps" ca
                INNER JOIN "AzureResource" ar ON ar."Id" = ca."Id"
                UNION ALL
                SELECT sd."Id" AS "ChildResourceId",
                       sd."SqlServerId" AS "ParentResourceId",
                       ar."ResourceGroupId"
                FROM "SqlDatabases" sd
                INNER JOIN "AzureResource" ar ON ar."Id" = sd."Id"
                UNION ALL
                SELECT ai."Id" AS "ChildResourceId",
                       ai."LogAnalyticsWorkspaceId" AS "ParentResourceId",
                       ar."ResourceGroupId"
                FROM "ApplicationInsights" ai
                INNER JOIN "AzureResource" ar ON ar."Id" = ai."Id";
                """);

            migrationBuilder.Sql("""
                CREATE OR REPLACE VIEW "vw_ResourceEnvironmentEntries" AS
                SELECT ar."ResourceGroupId", es."KeyVaultId" AS "ResourceId", es."EnvironmentName"
                FROM "KeyVaultEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."KeyVaultId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."RedisCacheId" AS "ResourceId", es."EnvironmentName"
                FROM "RedisCacheEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."RedisCacheId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."StorageAccountId" AS "ResourceId", es."EnvironmentName"
                FROM "StorageAccountEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."StorageAccountId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."AppServicePlanId" AS "ResourceId", es."EnvironmentName"
                FROM "AppServicePlanEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."AppServicePlanId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."WebAppId" AS "ResourceId", es."EnvironmentName"
                FROM "WebAppEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."WebAppId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."FunctionAppId" AS "ResourceId", es."EnvironmentName"
                FROM "FunctionAppEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."FunctionAppId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."AppConfigurationId" AS "ResourceId", es."EnvironmentName"
                FROM "AppConfigurationEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."AppConfigurationId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."ContainerAppEnvironmentId" AS "ResourceId", es."EnvironmentName"
                FROM "ContainerAppEnvironmentEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."ContainerAppEnvironmentId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."ContainerAppId" AS "ResourceId", es."EnvironmentName"
                FROM "ContainerAppEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."ContainerAppId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."LogAnalyticsWorkspaceId" AS "ResourceId", es."EnvironmentName"
                FROM "LogAnalyticsWorkspaceEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."LogAnalyticsWorkspaceId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."ApplicationInsightsId" AS "ResourceId", es."EnvironmentName"
                FROM "ApplicationInsightsEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."ApplicationInsightsId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."CosmosDbId" AS "ResourceId", es."EnvironmentName"
                FROM "CosmosDbEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."CosmosDbId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."SqlServerId" AS "ResourceId", es."EnvironmentName"
                FROM "SqlServerEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."SqlServerId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."SqlDatabaseId" AS "ResourceId", es."EnvironmentName"
                FROM "SqlDatabaseEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."SqlDatabaseId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."ServiceBusNamespaceId" AS "ResourceId", es."EnvironmentName"
                FROM "ServiceBusNamespaceEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."ServiceBusNamespaceId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."ContainerRegistryId" AS "ResourceId", es."EnvironmentName"
                FROM "ContainerRegistryEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."ContainerRegistryId"
                UNION ALL
                SELECT ar."ResourceGroupId", es."EventHubNamespaceId" AS "ResourceId", es."EnvironmentName"
                FROM "EventHubNamespaceEnvironmentSettings" es
                INNER JOIN "AzureResource" ar ON ar."Id" = es."EventHubNamespaceId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP VIEW IF EXISTS "vw_ChildToParentLinks";""");
            migrationBuilder.Sql("""DROP VIEW IF EXISTS "vw_ResourceEnvironmentEntries";""");
        }
    }
}

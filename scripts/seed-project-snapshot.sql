-- ============================================================
-- InfraFlowSculptor — Seed depuis le snapshot fb8699ea-ifs-project
-- PostgreSQL 17 / infraDb
-- Idempotent : ON CONFLICT (Id) DO NOTHING sur toutes les tables à PK
-- ============================================================

BEGIN;

-- ─────────────────────────────────────────────────────────────
-- 1. PROJET
-- ─────────────────────────────────────────────────────────────
INSERT INTO "Projects" ("Id", "Name", "Description", "DefaultNamingTemplate", "RepositoryMode", "AgentPoolName")
VALUES (
    'fb8699ea-f568-4afb-864b-e82d2efd0905',
    'Infra Flow Sculptor',
    NULL,
    '{name}-{resourceAbbr}{suffix}',
    'MonoRepo',
    'Default'
)
ON CONFLICT ("Id") DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 2. ENVIRONNEMENT : Development
-- ─────────────────────────────────────────────────────────────
INSERT INTO "ProjectEnvironments" (
    "Id", "ProjectId", "Name", "ShortName", "Prefix", "Suffix",
    "Location", "SubscriptionId", "Order", "RequiresApproval",
    "AzureResourceManagerConnection"
)
VALUES (
    '16e70ad4-a8da-434f-80c5-3000a2bca95b',
    'fb8699ea-f568-4afb-864b-e82d2efd0905',
    'Development', 'dev', 'dev-', '-dev',
    'FranceCentral',
    '83d0b022-c10f-40e7-8eb9-72f37ae7e283',
    0, false, 'ifs'
)
ON CONFLICT ("Id") DO NOTHING;

-- Tag environment=dev
INSERT INTO "ProjectEnvironmentTags" ("Name", "EnvironmentId", "Value")
VALUES ('environment', '16e70ad4-a8da-434f-80c5-3000a2bca95b', 'dev')
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 3. NAMING TEMPLATES PROJET
-- ─────────────────────────────────────────────────────────────
INSERT INTO "ProjectResourceNamingTemplates" ("Id", "ProjectId", "ResourceType", "Template")
VALUES
    (gen_random_uuid(), 'fb8699ea-f568-4afb-864b-e82d2efd0905', 'ResourceGroup',     '{resourceAbbr}-{name}{suffix}'),
    (gen_random_uuid(), 'fb8699ea-f568-4afb-864b-e82d2efd0905', 'StorageAccount',    '{name}{resourceAbbr}{envShort}'),
    (gen_random_uuid(), 'fb8699ea-f568-4afb-864b-e82d2efd0905', 'ContainerRegistry', '{name}{resourceAbbr}{envShort}')
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 4. INFRACONFIG : Core
-- ─────────────────────────────────────────────────────────────
INSERT INTO "InfrastructureConfigs" (
    "Id", "ProjectId", "Name", "DefaultNamingTemplate",
    "UseProjectNamingConventions", "AppPipelineMode"
)
VALUES (
    '4ce4be5e-e7d6-4084-94c8-77ff21be42ef',
    'fb8699ea-f568-4afb-864b-e82d2efd0905',
    'Core', NULL, true, 'Isolated'
)
ON CONFLICT ("Id") DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 5. RESOURCE GROUP : ifs-core
-- ─────────────────────────────────────────────────────────────
INSERT INTO "ResourceGroup" ("Id", "Name", "InfraConfigId", "Location")
VALUES (
    '5c87b514-a237-4040-a047-af431df4aa46',
    'ifs-core',
    '4ce4be5e-e7d6-4084-94c8-77ff21be42ef',
    'FranceCentral'
)
ON CONFLICT ("Id") DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 6. CONTAINER REGISTRY : ifs  (dans ifs-core)
-- ─────────────────────────────────────────────────────────────
INSERT INTO "AzureResource" ("Id", "ResourceGroupId", "Name", "Location", "CustomNameOverride", "AssignedUserAssignedIdentityId")
VALUES ('36ba74cb-c0a1-40a3-b224-e3be1c1b4b46', '5c87b514-a237-4040-a047-af431df4aa46', 'ifs', 'FranceCentral', NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "ContainerRegistries" ("Id")
VALUES ('36ba74cb-c0a1-40a3-b224-e3be1c1b4b46')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "ContainerRegistryEnvironmentSettings" (
    "Id", "ContainerRegistryId", "EnvironmentName",
    "Sku", "AdminUserEnabled", "PublicNetworkAccess", "ZoneRedundancy"
)
VALUES (
    gen_random_uuid(),
    '36ba74cb-c0a1-40a3-b224-e3be1c1b4b46',
    'Development', 'Basic', false, 'Enabled', false
)
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 7. LOG ANALYTICS WORKSPACE : ifs  (dans ifs-core)
-- ─────────────────────────────────────────────────────────────
INSERT INTO "AzureResource" ("Id", "ResourceGroupId", "Name", "Location", "CustomNameOverride", "AssignedUserAssignedIdentityId")
VALUES ('83b21c4d-584c-4e2e-8362-3ddf6aee73c2', '5c87b514-a237-4040-a047-af431df4aa46', 'ifs', 'FranceCentral', NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "LogAnalyticsWorkspaces" ("Id")
VALUES ('83b21c4d-584c-4e2e-8362-3ddf6aee73c2')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "LogAnalyticsWorkspaceEnvironmentSettings" (
    "Id", "LogAnalyticsWorkspaceId", "EnvironmentName",
    "Sku", "RetentionInDays", "DailyQuotaGb"
)
VALUES (
    gen_random_uuid(),
    '83b21c4d-584c-4e2e-8362-3ddf6aee73c2',
    'Development', 'Free', 30, 2
)
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 8. INFRACONFIG : Infra Flow Sculptor
-- ─────────────────────────────────────────────────────────────
INSERT INTO "InfrastructureConfigs" (
    "Id", "ProjectId", "Name", "DefaultNamingTemplate",
    "UseProjectNamingConventions", "AppPipelineMode"
)
VALUES (
    '3afe7f4f-e113-4ba8-9d83-999dd7dc22b4',
    'fb8699ea-f568-4afb-864b-e82d2efd0905',
    'Infra Flow Sculptor', NULL, true, 'Isolated'
)
ON CONFLICT ("Id") DO NOTHING;

-- Cross-config reference : IFS → LogAnalyticsWorkspace de Core
INSERT INTO "CrossConfigResourceReferences" ("Id", "TargetConfigId", "TargetResourceId", "InfraConfigId")
VALUES (
    gen_random_uuid(),
    '4ce4be5e-e7d6-4084-94c8-77ff21be42ef',
    '83b21c4d-584c-4e2e-8362-3ddf6aee73c2',
    '3afe7f4f-e113-4ba8-9d83-999dd7dc22b4'
)
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 9. RESOURCE GROUP : ifs
-- ─────────────────────────────────────────────────────────────
INSERT INTO "ResourceGroup" ("Id", "Name", "InfraConfigId", "Location")
VALUES (
    '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9',
    'ifs',
    '3afe7f4f-e113-4ba8-9d83-999dd7dc22b4',
    'FranceCentral'
)
ON CONFLICT ("Id") DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 10. KEY VAULT : ifs
-- ─────────────────────────────────────────────────────────────
INSERT INTO "AzureResource" ("Id", "ResourceGroupId", "Name", "Location", "CustomNameOverride", "AssignedUserAssignedIdentityId")
VALUES ('fc210d60-9d8a-4899-9f9e-07dced5871c5', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs', 'FranceCentral', NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "KeyVaults" (
    "Id", "EnablePurgeProtection", "EnableRbacAuthorization", "EnableSoftDelete",
    "EnabledForDeployment", "EnabledForDiskEncryption", "EnabledForTemplateDeployment"
)
VALUES ('fc210d60-9d8a-4899-9f9e-07dced5871c5', true, true, true, false, false, false)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "KeyVaultEnvironmentSettings" ("Id", "KeyVaultId", "EnvironmentName", "Sku")
VALUES (gen_random_uuid(), 'fc210d60-9d8a-4899-9f9e-07dced5871c5', 'Development', 'Standard')
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 11. STORAGE ACCOUNT : ifs
-- ─────────────────────────────────────────────────────────────
INSERT INTO "AzureResource" ("Id", "ResourceGroupId", "Name", "Location", "CustomNameOverride", "AssignedUserAssignedIdentityId")
VALUES ('efe669ac-71a9-4884-a6b1-cf16583bbf37', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs', 'FranceCentral', NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "StorageAccounts" (
    "Id", "AccessTier", "AllowBlobPublicAccess",
    "EnableHttpsTrafficOnly", "Kind", "MinimumTlsVersion"
)
VALUES ('efe669ac-71a9-4884-a6b1-cf16583bbf37', 'Hot', false, true, 'StorageV2', 'Tls12')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "StorageAccountEnvironmentSettings" ("Id", "StorageAccountId", "EnvironmentName", "Sku")
VALUES (gen_random_uuid(), 'efe669ac-71a9-4884-a6b1-cf16583bbf37', 'Development', 'Standard_LRS')
ON CONFLICT DO NOTHING;

-- Blob container : bicep-output
INSERT INTO "BlobContainers" ("Id", "StorageAccountId", "Name", "PublicAccess")
VALUES (gen_random_uuid(), 'efe669ac-71a9-4884-a6b1-cf16583bbf37', 'bicep-output', 'None')
ON CONFLICT DO NOTHING;

-- Lifecycle rule : clean-ifs (1 jour de TTL)
INSERT INTO "BlobLifecycleRules" ("Id", "StorageAccountId", "RuleName", "ContainerNames", "TimeToLiveInDays")
VALUES (gen_random_uuid(), 'efe669ac-71a9-4884-a6b1-cf16583bbf37', 'clean-ifs', '["bicep-output"]'::jsonb, 1)
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 12. SQL SERVER : ifs
-- ─────────────────────────────────────────────────────────────
INSERT INTO "AzureResource" ("Id", "ResourceGroupId", "Name", "Location", "CustomNameOverride", "AssignedUserAssignedIdentityId")
VALUES ('9700666f-4771-46f9-aaab-74d9370eee59', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs', 'FranceCentral', NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "SqlServers" ("Id", "Version", "AdministratorLogin")
VALUES ('9700666f-4771-46f9-aaab-74d9370eee59', 'V12', 'sqladmin')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "SqlServerEnvironmentSettings" ("Id", "SqlServerId", "EnvironmentName", "MinimalTlsVersion")
VALUES (gen_random_uuid(), '9700666f-4771-46f9-aaab-74d9370eee59', 'Development', '1.2')
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 13. SQL DATABASE : ifs
-- ─────────────────────────────────────────────────────────────
INSERT INTO "AzureResource" ("Id", "ResourceGroupId", "Name", "Location", "CustomNameOverride", "AssignedUserAssignedIdentityId")
VALUES ('0ae0ab1e-a076-468d-a7af-41fd6b4d3d20', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs', 'FranceCentral', NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "SqlDatabases" ("Id", "SqlServerId", "Collation")
VALUES ('0ae0ab1e-a076-468d-a7af-41fd6b4d3d20', '9700666f-4771-46f9-aaab-74d9370eee59', 'SQL_Latin1_General_CP1_CI_AS')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "SqlDatabaseEnvironmentSettings" (
    "Id", "SqlDatabaseId", "EnvironmentName", "Sku", "MaxSizeGb", "ZoneRedundant"
)
VALUES (gen_random_uuid(), '0ae0ab1e-a076-468d-a7af-41fd6b4d3d20', 'Development', 'Basic', 5, false)
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 14. APPLICATION INSIGHTS : ifs  (référence LAW Core)
-- ─────────────────────────────────────────────────────────────
INSERT INTO "AzureResource" ("Id", "ResourceGroupId", "Name", "Location", "CustomNameOverride", "AssignedUserAssignedIdentityId")
VALUES ('e8bdb228-bc60-4de6-9d88-4d549b5a64bb', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs', 'FranceCentral', NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "ApplicationInsights" ("Id", "LogAnalyticsWorkspaceId")
VALUES ('e8bdb228-bc60-4de6-9d88-4d549b5a64bb', '83b21c4d-584c-4e2e-8362-3ddf6aee73c2')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "ApplicationInsightsEnvironmentSettings" (
    "Id", "ApplicationInsightsId", "EnvironmentName",
    "SamplingPercentage", "RetentionInDays", "DisableIpMasking", "DisableLocalAuth", "IngestionMode"
)
VALUES (
    gen_random_uuid(),
    'e8bdb228-bc60-4de6-9d88-4d549b5a64bb',
    'Development', 100, 30, true, false, 'LogAnalytics'
)
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 15. CONTAINER APP ENVIRONMENT : ifs  (référence LAW Core)
-- ─────────────────────────────────────────────────────────────
INSERT INTO "AzureResource" ("Id", "ResourceGroupId", "Name", "Location", "CustomNameOverride", "AssignedUserAssignedIdentityId")
VALUES ('37cfd530-1f07-442a-849c-4c030bb147a4', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs', 'FranceCentral', NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "ContainerAppEnvironments" ("Id", "LogAnalyticsWorkspaceId")
VALUES ('37cfd530-1f07-442a-849c-4c030bb147a4', '83b21c4d-584c-4e2e-8362-3ddf6aee73c2')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "ContainerAppEnvironmentEnvironmentSettings" (
    "Id", "ContainerAppEnvironmentId", "EnvironmentName",
    "Sku", "WorkloadProfileType", "InternalLoadBalancerEnabled", "ZoneRedundancyEnabled"
)
VALUES (
    gen_random_uuid(),
    '37cfd530-1f07-442a-849c-4c030bb147a4',
    'Development', 'Consumption', 'Consumption', false, false
)
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 16. CONTAINER APP : ifs-api
-- ─────────────────────────────────────────────────────────────
INSERT INTO "AzureResource" ("Id", "ResourceGroupId", "Name", "Location", "CustomNameOverride", "AssignedUserAssignedIdentityId")
VALUES ('4615c4e9-1584-472d-b158-bdb41c49e4ed', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs-api', 'FranceCentral', NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "ContainerApps" (
    "Id", "ContainerAppEnvironmentId", "ContainerRegistryId",
    "DockerImageName", "DockerfilePath", "ApplicationName"
)
VALUES ('4615c4e9-1584-472d-b158-bdb41c49e4ed', '37cfd530-1f07-442a-849c-4c030bb147a4', NULL, NULL, NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "ContainerAppEnvironmentSettings" (
    "Id", "ContainerAppId", "EnvironmentName",
    "CpuCores", "MemoryGi", "MinReplicas", "MaxReplicas",
    "IngressEnabled", "IngressTargetPort", "IngressExternal", "TransportMethod"
)
VALUES (
    gen_random_uuid(),
    '4615c4e9-1584-472d-b158-bdb41c49e4ed',
    'Development', '0.25', '0.5Gi', 0, 1, true, 80, true, 'auto'
)
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 17. CONTAINER APP : ifs-frontend
-- ─────────────────────────────────────────────────────────────
INSERT INTO "AzureResource" ("Id", "ResourceGroupId", "Name", "Location", "CustomNameOverride", "AssignedUserAssignedIdentityId")
VALUES ('dda2e846-de85-4739-ba0c-ec15f63e48c7', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs-frontend', 'FranceCentral', NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "ContainerApps" (
    "Id", "ContainerAppEnvironmentId", "ContainerRegistryId",
    "DockerImageName", "DockerfilePath", "ApplicationName"
)
VALUES ('dda2e846-de85-4739-ba0c-ec15f63e48c7', '37cfd530-1f07-442a-849c-4c030bb147a4', NULL, NULL, NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "ContainerAppEnvironmentSettings" (
    "Id", "ContainerAppId", "EnvironmentName",
    "CpuCores", "MemoryGi", "MinReplicas", "MaxReplicas",
    "IngressEnabled", "IngressTargetPort", "IngressExternal", "TransportMethod"
)
VALUES (
    gen_random_uuid(),
    'dda2e846-de85-4739-ba0c-ec15f63e48c7',
    'Development', '0.25', '0.5Gi', 0, 1, true, 80, true, 'auto'
)
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 18. ROLE ASSIGNMENT : ifs-api → KeyVault (Key Vault Secrets User)
-- ─────────────────────────────────────────────────────────────
INSERT INTO "RoleAssignments" (
    "Id", "SourceResourceId", "TargetResourceId",
    "ManagedIdentityType", "RoleDefinitionId", "UserAssignedIdentityId"
)
VALUES (
    gen_random_uuid(),
    '4615c4e9-1584-472d-b158-bdb41c49e4ed',
    'fc210d60-9d8a-4899-9f9e-07dced5871c5',
    'SystemAssigned',
    '4633458b-17de-408a-b874-0445c86b69e6',
    NULL
)
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────
-- 19. APP SETTINGS : ifs-api
-- ─────────────────────────────────────────────────────────────
-- 19a. APPLICATIONINSIGHTS_CONNECTION_STRING (output reference)
INSERT INTO "AppSettings" (
    "Id", "ResourceId", "Name",
    "SourceResourceId", "SourceOutputName",
    "KeyVaultResourceId", "SecretName", "SecretValueAssignment",
    "PipelineVariableName", "VariableGroupId"
)
VALUES (
    gen_random_uuid(),
    '4615c4e9-1584-472d-b158-bdb41c49e4ed',
    'APPLICATIONINSIGHTS_CONNECTION_STRING',
    'e8bdb228-bc60-4de6-9d88-4d549b5a64bb',
    'connectionString',
    NULL, NULL, NULL, NULL, NULL
)
ON CONFLICT DO NOTHING;

-- 19b. JwtSettings__Secret (KV secret, ViaBicepparam)
INSERT INTO "AppSettings" (
    "Id", "ResourceId", "Name",
    "SourceResourceId", "SourceOutputName",
    "KeyVaultResourceId", "SecretName", "SecretValueAssignment",
    "PipelineVariableName", "VariableGroupId"
)
VALUES (
    gen_random_uuid(),
    '4615c4e9-1584-472d-b158-bdb41c49e4ed',
    'JwtSettings__Secret',
    NULL, NULL,
    'fc210d60-9d8a-4899-9f9e-07dced5871c5',
    'JWT_SECRET',
    'ViaBicepparam',
    'jwt-secret',
    NULL
)
ON CONFLICT DO NOTHING;

COMMIT;

-- ─────────────────────────────────────────────────────────────
-- Vérification rapide
-- ─────────────────────────────────────────────────────────────
SELECT 'Projects'              AS table_name, COUNT(*) FROM "Projects"              WHERE "Id" = 'fb8699ea-f568-4afb-864b-e82d2efd0905'
UNION ALL
SELECT 'ProjectEnvironments',               COUNT(*) FROM "ProjectEnvironments"    WHERE "ProjectId" = 'fb8699ea-f568-4afb-864b-e82d2efd0905'
UNION ALL
SELECT 'InfrastructureConfigs',             COUNT(*) FROM "InfrastructureConfigs"  WHERE "ProjectId" = 'fb8699ea-f568-4afb-864b-e82d2efd0905'
UNION ALL
SELECT 'ResourceGroups',                    COUNT(*) FROM "ResourceGroup"          WHERE "InfraConfigId" IN ('4ce4be5e-e7d6-4084-94c8-77ff21be42ef','3afe7f4f-e113-4ba8-9d83-999dd7dc22b4')
UNION ALL
SELECT 'AzureResources',                    COUNT(*) FROM "AzureResource"          WHERE "ResourceGroupId" IN ('5c87b514-a237-4040-a047-af431df4aa46','8c99a6b4-94d1-4979-bfe3-e8ba22d711d9')
UNION ALL
SELECT 'AppSettings',                       COUNT(*) FROM "AppSettings"            WHERE "ResourceId" = '4615c4e9-1584-472d-b158-bdb41c49e4ed'
UNION ALL
SELECT 'RoleAssignments',                   COUNT(*) FROM "RoleAssignments"        WHERE "SourceResourceId" = '4615c4e9-1584-472d-b158-bdb41c49e4ed';

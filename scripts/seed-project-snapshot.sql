-- ============================================================
-- InfraFlowSculptor — direct local seed for snapshot fb8699ea
-- Source snapshot: docs/project-snapshots/fb8699ea-ifs-project.md
-- Synchronization rule: update this SQL and the snapshot together.
-- Target: PostgreSQL 17 / infraDb
-- Idempotent seed for a live current-schema developer database.
-- ============================================================

BEGIN;

INSERT INTO "Projects" ("Id", "Name", "Description", "DefaultNamingTemplate", "LayoutPreset", "AgentPoolName")
VALUES (
    'fb8699ea-f568-4afb-864b-e82d2efd0905',
    'Infra Flow Sculptor',
    NULL,
    '{name}-{resourceAbbr}{suffix}',
    'SplitInfraCode',
    'Default'
)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO project_members ("Id", "UserId", "Role", "ProjectId")
SELECT gen_random_uuid(), u."Id", 'Owner', 'fb8699ea-f568-4afb-864b-e82d2efd0905'
FROM "User" u
WHERE NOT EXISTS (
    SELECT 1
    FROM project_members pm
    WHERE pm."ProjectId" = 'fb8699ea-f568-4afb-864b-e82d2efd0905'
      AND pm."UserId" = u."Id")
LIMIT 1;

INSERT INTO "ProjectEnvironments" (
    "Id", "ProjectId", "Name", "ShortName", "Prefix", "Suffix",
    "Location", "SubscriptionId", "Order", "RequiresApproval", "AzureResourceManagerConnection")
VALUES (
    '16e70ad4-a8da-434f-80c5-3000a2bca95b',
    'fb8699ea-f568-4afb-864b-e82d2efd0905',
    'Development', 'dev', 'dev-', '-dev',
    'FranceCentral',
    '83d0b022-c10f-40e7-8eb9-72f37ae7e283',
    0, false, 'ifs')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "ProjectEnvironmentTags" ("Name", "EnvironmentId", "Value")
SELECT 'environment', '16e70ad4-a8da-434f-80c5-3000a2bca95b', 'dev'
WHERE NOT EXISTS (
    SELECT 1 FROM "ProjectEnvironmentTags"
    WHERE "EnvironmentId" = '16e70ad4-a8da-434f-80c5-3000a2bca95b'
      AND "Name" = 'environment'
      AND "Value" = 'dev');

INSERT INTO "ProjectRepositories" ("Id", "ProjectId", "Alias", "ProviderType", "RepositoryUrl", "Owner", "RepositoryName", "DefaultBranch", "ContentKinds")
SELECT gen_random_uuid(), 'fb8699ea-f568-4afb-864b-e82d2efd0905', 'infra', 'AzureDevOps',
       'https://floriandrevet0332@dev.azure.com/floriandrevet0332/Infra%20Flow%20Sculptor/_git/ifs',
       'floriandrevet0332/Infra Flow Sculptor', 'ifs', 'main', 'Infrastructure'
WHERE NOT EXISTS (
    SELECT 1 FROM "ProjectRepositories"
    WHERE "ProjectId" = 'fb8699ea-f568-4afb-864b-e82d2efd0905' AND "Alias" = 'infra');

INSERT INTO "ProjectRepositories" ("Id", "ProjectId", "Alias", "ProviderType", "RepositoryUrl", "Owner", "RepositoryName", "DefaultBranch", "ContentKinds")
SELECT gen_random_uuid(), 'fb8699ea-f568-4afb-864b-e82d2efd0905', 'code', 'AzureDevOps',
       'https://floriandrevet0332@dev.azure.com/floriandrevet0332/Infra%20Flow%20Sculptor/_git/Infra%20Flow%20Sculptor',
       'floriandrevet0332/Infra Flow Sculptor', 'Infra Flow Sculptor', 'main', 'ApplicationCode'
WHERE NOT EXISTS (
    SELECT 1 FROM "ProjectRepositories"
    WHERE "ProjectId" = 'fb8699ea-f568-4afb-864b-e82d2efd0905' AND "Alias" = 'code');

INSERT INTO "ProjectResourceNamingTemplates" ("Id", "ProjectId", "ResourceType", "Template")
SELECT gen_random_uuid(), 'fb8699ea-f568-4afb-864b-e82d2efd0905', 'ResourceGroup', '{resourceAbbr}-{name}{suffix}'
WHERE NOT EXISTS (
    SELECT 1 FROM "ProjectResourceNamingTemplates"
    WHERE "ProjectId" = 'fb8699ea-f568-4afb-864b-e82d2efd0905' AND "ResourceType" = 'ResourceGroup');

INSERT INTO "ProjectResourceNamingTemplates" ("Id", "ProjectId", "ResourceType", "Template")
SELECT gen_random_uuid(), 'fb8699ea-f568-4afb-864b-e82d2efd0905', 'StorageAccount', '{name}{resourceAbbr}{envShort}'
WHERE NOT EXISTS (
    SELECT 1 FROM "ProjectResourceNamingTemplates"
    WHERE "ProjectId" = 'fb8699ea-f568-4afb-864b-e82d2efd0905' AND "ResourceType" = 'StorageAccount');

INSERT INTO "ProjectResourceNamingTemplates" ("Id", "ProjectId", "ResourceType", "Template")
SELECT gen_random_uuid(), 'fb8699ea-f568-4afb-864b-e82d2efd0905', 'ContainerRegistry', '{name}{resourceAbbr}{envShort}'
WHERE NOT EXISTS (
    SELECT 1 FROM "ProjectResourceNamingTemplates"
    WHERE "ProjectId" = 'fb8699ea-f568-4afb-864b-e82d2efd0905' AND "ResourceType" = 'ContainerRegistry');

INSERT INTO "InfrastructureConfigs" ("Id", "ProjectId", "Name", "DefaultNamingTemplate", "UseProjectNamingConventions", "AppPipelineMode", "LayoutMode")
VALUES
    ('4ce4be5e-e7d6-4084-94c8-77ff21be42ef', 'fb8699ea-f568-4afb-864b-e82d2efd0905', 'Core', NULL, true, 'Isolated', NULL),
    ('3afe7f4f-e113-4ba8-9d83-999dd7dc22b4', 'fb8699ea-f568-4afb-864b-e82d2efd0905', 'Infra Flow Sculptor', NULL, true, 'Isolated', NULL)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "CrossConfigResourceReferences" ("Id", "TargetConfigId", "TargetResourceId", "InfraConfigId")
SELECT gen_random_uuid(), '4ce4be5e-e7d6-4084-94c8-77ff21be42ef', '83b21c4d-584c-4e2e-8362-3ddf6aee73c2', '3afe7f4f-e113-4ba8-9d83-999dd7dc22b4'
WHERE NOT EXISTS (
    SELECT 1 FROM "CrossConfigResourceReferences"
    WHERE "TargetConfigId" = '4ce4be5e-e7d6-4084-94c8-77ff21be42ef'
      AND "TargetResourceId" = '83b21c4d-584c-4e2e-8362-3ddf6aee73c2'
      AND "InfraConfigId" = '3afe7f4f-e113-4ba8-9d83-999dd7dc22b4');

INSERT INTO "ResourceGroup" ("Id", "Name", "InfraConfigId", "Location")
VALUES
    ('5c87b514-a237-4040-a047-af431df4aa46', 'ifs-core', '4ce4be5e-e7d6-4084-94c8-77ff21be42ef', 'FranceCentral'),
    ('8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs', '3afe7f4f-e113-4ba8-9d83-999dd7dc22b4', 'FranceCentral')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "AzureResource" ("Id", "ResourceType", "ResourceGroupId", "Name", "Location", "CustomNameOverride", "AssignedUserAssignedIdentityId")
VALUES
    ('36ba74cb-c0a1-40a3-b224-e3be1c1b4b46', 'ContainerRegistry', '5c87b514-a237-4040-a047-af431df4aa46', 'infraflowsculptor', 'FranceCentral', NULL, NULL),
    ('83b21c4d-584c-4e2e-8362-3ddf6aee73c2', 'LogAnalyticsWorkspace', '5c87b514-a237-4040-a047-af431df4aa46', 'ifs', 'FranceCentral', NULL, NULL),
    ('fc210d60-9d8a-4899-9f9e-07dced5871c5', 'KeyVault', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs', 'FranceCentral', NULL, NULL),
    ('efe669ac-71a9-4884-a6b1-cf16583bbf37', 'StorageAccount', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs', 'FranceCentral', NULL, NULL),
    ('9700666f-4771-46f9-aaab-74d9370eee59', 'SqlServer', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'infra-flow', 'FranceCentral', NULL, NULL),
    ('0ae0ab1e-a076-468d-a7af-41fd6b4d3d20', 'SqlDatabase', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs', 'FranceCentral', NULL, NULL),
    ('e8bdb228-bc60-4de6-9d88-4d549b5a64bb', 'ApplicationInsights', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs', 'FranceCentral', NULL, NULL),
    ('37cfd530-1f07-442a-849c-4c030bb147a4', 'ContainerAppEnvironment', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs', 'FranceCentral', NULL, NULL),
    ('38cb3416-b0c9-482e-80f9-2332f77974db', 'UserAssignedIdentity', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'backend', 'FranceCentral', NULL, NULL),
    ('90097d4d-74a6-4d23-b94a-0392c57f7d14', 'UserAssignedIdentity', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'frontend', 'FranceCentral', NULL, NULL),
    ('4615c4e9-1584-472d-b158-bdb41c49e4ed', 'ContainerApp', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs-api', 'FranceCentral', NULL, '38cb3416-b0c9-482e-80f9-2332f77974db'),
    ('dda2e846-de85-4739-ba0c-ec15f63e48c7', 'ContainerApp', '8c99a6b4-94d1-4979-bfe3-e8ba22d711d9', 'ifs-frontend', 'FranceCentral', NULL, '90097d4d-74a6-4d23-b94a-0392c57f7d14')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "ContainerRegistries" ("Id") VALUES ('36ba74cb-c0a1-40a3-b224-e3be1c1b4b46') ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "LogAnalyticsWorkspaces" ("Id") VALUES ('83b21c4d-584c-4e2e-8362-3ddf6aee73c2') ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "KeyVaults" ("Id", "EnablePurgeProtection", "EnableRbacAuthorization", "EnableSoftDelete", "EnabledForDeployment", "EnabledForDiskEncryption", "EnabledForTemplateDeployment")
VALUES ('fc210d60-9d8a-4899-9f9e-07dced5871c5', true, true, true, false, false, false)
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "StorageAccounts" ("Id", "Kind", "AccessTier", "AllowBlobPublicAccess", "EnableHttpsTrafficOnly", "MinimumTlsVersion")
VALUES ('efe669ac-71a9-4884-a6b1-cf16583bbf37', 'StorageV2', 'Hot', false, true, 'Tls12')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "SqlServers" ("Id", "Version", "AdministratorLogin")
VALUES ('9700666f-4771-46f9-aaab-74d9370eee59', 'V12', 'sqladmin')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "SqlDatabases" ("Id", "SqlServerId", "Collation")
VALUES ('0ae0ab1e-a076-468d-a7af-41fd6b4d3d20', '9700666f-4771-46f9-aaab-74d9370eee59', 'SQL_Latin1_General_CP1_CI_AS')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ApplicationInsights" ("Id", "LogAnalyticsWorkspaceId")
VALUES ('e8bdb228-bc60-4de6-9d88-4d549b5a64bb', '83b21c4d-584c-4e2e-8362-3ddf6aee73c2')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ContainerAppEnvironments" ("Id", "LogAnalyticsWorkspaceId")
VALUES ('37cfd530-1f07-442a-849c-4c030bb147a4', '83b21c4d-584c-4e2e-8362-3ddf6aee73c2')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserAssignedIdentities" ("Id") VALUES ('38cb3416-b0c9-482e-80f9-2332f77974db') ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserAssignedIdentities" ("Id") VALUES ('90097d4d-74a6-4d23-b94a-0392c57f7d14') ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ContainerApps" ("Id", "ContainerAppEnvironmentId", "ContainerRegistryId", "AcrAuthMode", "AcrPullIdentityId", "DockerImageName", "DockerfilePath", "ApplicationName")
VALUES
    ('4615c4e9-1584-472d-b158-bdb41c49e4ed', '37cfd530-1f07-442a-849c-4c030bb147a4', '36ba74cb-c0a1-40a3-b224-e3be1c1b4b46', 'ManagedIdentity', '38cb3416-b0c9-482e-80f9-2332f77974db', 'ifs/backend', 'src/Api/InfraFlowSculptor.Api/Dockerfile', NULL),
    ('dda2e846-de85-4739-ba0c-ec15f63e48c7', '37cfd530-1f07-442a-849c-4c030bb147a4', '36ba74cb-c0a1-40a3-b224-e3be1c1b4b46', 'ManagedIdentity', '90097d4d-74a6-4d23-b94a-0392c57f7d14', 'ifs/frontend', 'src/Front/Dockerfile', 'Ifs frontend')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "ContainerRegistryEnvironmentSettings" ("Id", "ContainerRegistryId", "EnvironmentName", "Sku", "AdminUserEnabled", "PublicNetworkAccess", "ZoneRedundancy")
SELECT gen_random_uuid(), '36ba74cb-c0a1-40a3-b224-e3be1c1b4b46', 'Development', 'Basic', false, 'Enabled', false
WHERE NOT EXISTS (
    SELECT 1 FROM "ContainerRegistryEnvironmentSettings"
    WHERE "ContainerRegistryId" = '36ba74cb-c0a1-40a3-b224-e3be1c1b4b46' AND "EnvironmentName" = 'Development');

INSERT INTO "LogAnalyticsWorkspaceEnvironmentSettings" ("Id", "LogAnalyticsWorkspaceId", "EnvironmentName", "Sku", "RetentionInDays", "DailyQuotaGb")
SELECT gen_random_uuid(), '83b21c4d-584c-4e2e-8362-3ddf6aee73c2', 'Development', 'Free', 30, 2
WHERE NOT EXISTS (
    SELECT 1 FROM "LogAnalyticsWorkspaceEnvironmentSettings"
    WHERE "LogAnalyticsWorkspaceId" = '83b21c4d-584c-4e2e-8362-3ddf6aee73c2' AND "EnvironmentName" = 'Development');

INSERT INTO "KeyVaultEnvironmentSettings" ("Id", "KeyVaultId", "EnvironmentName", "Sku")
SELECT gen_random_uuid(), 'fc210d60-9d8a-4899-9f9e-07dced5871c5', 'Development', 'Standard'
WHERE NOT EXISTS (
    SELECT 1 FROM "KeyVaultEnvironmentSettings"
    WHERE "KeyVaultId" = 'fc210d60-9d8a-4899-9f9e-07dced5871c5' AND "EnvironmentName" = 'Development');

INSERT INTO "StorageAccountEnvironmentSettings" ("Id", "StorageAccountId", "EnvironmentName", "Sku")
SELECT gen_random_uuid(), 'efe669ac-71a9-4884-a6b1-cf16583bbf37', 'Development', 'Standard_LRS'
WHERE NOT EXISTS (
    SELECT 1 FROM "StorageAccountEnvironmentSettings"
    WHERE "StorageAccountId" = 'efe669ac-71a9-4884-a6b1-cf16583bbf37' AND "EnvironmentName" = 'Development');

INSERT INTO "BlobContainers" ("Id", "StorageAccountId", "Name", "PublicAccess")
SELECT gen_random_uuid(), 'efe669ac-71a9-4884-a6b1-cf16583bbf37', 'bicep-output', 'None'
WHERE NOT EXISTS (
    SELECT 1 FROM "BlobContainers"
    WHERE "StorageAccountId" = 'efe669ac-71a9-4884-a6b1-cf16583bbf37' AND "Name" = 'bicep-output');

INSERT INTO "BlobLifecycleRules" ("Id", "StorageAccountId", "RuleName", "ContainerNames", "TimeToLiveInDays")
SELECT gen_random_uuid(), 'efe669ac-71a9-4884-a6b1-cf16583bbf37', 'clean-ifs', '["bicep-output"]'::jsonb, 1
WHERE NOT EXISTS (
    SELECT 1 FROM "BlobLifecycleRules"
    WHERE "StorageAccountId" = 'efe669ac-71a9-4884-a6b1-cf16583bbf37' AND "RuleName" = 'clean-ifs');

INSERT INTO "SqlServerEnvironmentSettings" ("Id", "SqlServerId", "EnvironmentName", "MinimalTlsVersion")
SELECT gen_random_uuid(), '9700666f-4771-46f9-aaab-74d9370eee59', 'Development', '1.2'
WHERE NOT EXISTS (
    SELECT 1 FROM "SqlServerEnvironmentSettings"
    WHERE "SqlServerId" = '9700666f-4771-46f9-aaab-74d9370eee59' AND "EnvironmentName" = 'Development');

INSERT INTO "SqlDatabaseEnvironmentSettings" ("Id", "SqlDatabaseId", "EnvironmentName", "Sku", "MaxSizeGb", "ZoneRedundant")
SELECT gen_random_uuid(), '0ae0ab1e-a076-468d-a7af-41fd6b4d3d20', 'Development', 'Basic', 5, false
WHERE NOT EXISTS (
    SELECT 1 FROM "SqlDatabaseEnvironmentSettings"
    WHERE "SqlDatabaseId" = '0ae0ab1e-a076-468d-a7af-41fd6b4d3d20' AND "EnvironmentName" = 'Development');

INSERT INTO "ApplicationInsightsEnvironmentSettings" ("Id", "ApplicationInsightsId", "EnvironmentName", "SamplingPercentage", "RetentionInDays", "DisableIpMasking", "DisableLocalAuth", "IngestionMode")
SELECT gen_random_uuid(), 'e8bdb228-bc60-4de6-9d88-4d549b5a64bb', 'Development', 100, 30, true, false, 'LogAnalytics'
WHERE NOT EXISTS (
    SELECT 1 FROM "ApplicationInsightsEnvironmentSettings"
    WHERE "ApplicationInsightsId" = 'e8bdb228-bc60-4de6-9d88-4d549b5a64bb' AND "EnvironmentName" = 'Development');

INSERT INTO "ContainerAppEnvironmentEnvironmentSettings" ("Id", "ContainerAppEnvironmentId", "EnvironmentName", "Sku", "WorkloadProfileType", "InternalLoadBalancerEnabled", "ZoneRedundancyEnabled")
SELECT gen_random_uuid(), '37cfd530-1f07-442a-849c-4c030bb147a4', 'Development', 'Consumption', 'Consumption', false, false
WHERE NOT EXISTS (
    SELECT 1 FROM "ContainerAppEnvironmentEnvironmentSettings"
    WHERE "ContainerAppEnvironmentId" = '37cfd530-1f07-442a-849c-4c030bb147a4' AND "EnvironmentName" = 'Development');

INSERT INTO "ContainerAppEnvironmentSettings" (
    "Id", "ContainerAppId", "EnvironmentName", "CpuCores", "MemoryGi", "MinReplicas", "MaxReplicas",
    "IngressEnabled", "IngressTargetPort", "IngressExternal", "TransportMethod",
    "ReadinessProbePath", "ReadinessProbePort", "LivenessProbePath", "LivenessProbePort", "StartupProbePath", "StartupProbePort")
SELECT gen_random_uuid(), '4615c4e9-1584-472d-b158-bdb41c49e4ed', 'Development', '0.25', '0.5Gi', 0, 1,
       true, 80, true, 'auto',
       '/healthz/ready', 8080, NULL, NULL, '/healthz/startup', 8080
WHERE NOT EXISTS (
    SELECT 1 FROM "ContainerAppEnvironmentSettings"
    WHERE "ContainerAppId" = '4615c4e9-1584-472d-b158-bdb41c49e4ed' AND "EnvironmentName" = 'Development');

INSERT INTO "ContainerAppEnvironmentSettings" (
    "Id", "ContainerAppId", "EnvironmentName", "CpuCores", "MemoryGi", "MinReplicas", "MaxReplicas",
    "IngressEnabled", "IngressTargetPort", "IngressExternal", "TransportMethod",
    "ReadinessProbePath", "ReadinessProbePort", "LivenessProbePath", "LivenessProbePort", "StartupProbePath", "StartupProbePort")
SELECT gen_random_uuid(), 'dda2e846-de85-4739-ba0c-ec15f63e48c7', 'Development', '0.25', '0.5Gi', 0, 1,
       true, 81, true, 'auto',
       '/healthz/ready', 8080, '/healthz/live', 8080, '/healthz/startup', 8080
WHERE NOT EXISTS (
    SELECT 1 FROM "ContainerAppEnvironmentSettings"
    WHERE "ContainerAppId" = 'dda2e846-de85-4739-ba0c-ec15f63e48c7' AND "EnvironmentName" = 'Development');

INSERT INTO "CustomDomains" ("Id", "ResourceId", "EnvironmentName", "DomainName", "CertificateMode", "KeyVaultUrl", "ManagedIdentityResourceId", "CertificateName", "DnsValidationStatus")
SELECT gen_random_uuid(), 'dda2e846-de85-4739-ba0c-ec15f63e48c7', 'Development', 'infraflowsculptor.fr', 'ManagedCertificate', NULL, NULL, NULL, 'Pending'
WHERE NOT EXISTS (
    SELECT 1 FROM "CustomDomains"
    WHERE "ResourceId" = 'dda2e846-de85-4739-ba0c-ec15f63e48c7'
      AND "EnvironmentName" = 'Development'
      AND "DomainName" = 'infraflowsculptor.fr');

INSERT INTO "RoleAssignments" ("Id", "SourceResourceId", "TargetResourceId", "ManagedIdentityType", "RoleDefinitionId", "UserAssignedIdentityId")
SELECT gen_random_uuid(), '4615c4e9-1584-472d-b158-bdb41c49e4ed', 'fc210d60-9d8a-4899-9f9e-07dced5871c5', 'SystemAssigned', '4633458b-17de-408a-b874-0445c86b69e6', NULL
WHERE NOT EXISTS (
    SELECT 1 FROM "RoleAssignments"
    WHERE "SourceResourceId" = '4615c4e9-1584-472d-b158-bdb41c49e4ed'
      AND "TargetResourceId" = 'fc210d60-9d8a-4899-9f9e-07dced5871c5'
      AND "ManagedIdentityType" = 'SystemAssigned'
      AND "RoleDefinitionId" = '4633458b-17de-408a-b874-0445c86b69e6'
      AND "UserAssignedIdentityId" IS NULL);

INSERT INTO "RoleAssignments" ("Id", "SourceResourceId", "TargetResourceId", "ManagedIdentityType", "RoleDefinitionId", "UserAssignedIdentityId")
SELECT gen_random_uuid(), 'dda2e846-de85-4739-ba0c-ec15f63e48c7', '36ba74cb-c0a1-40a3-b224-e3be1c1b4b46', 'UserAssigned', '7f951dda-4ed3-4680-a7ca-43fe172d538e', '90097d4d-74a6-4d23-b94a-0392c57f7d14'
WHERE NOT EXISTS (
    SELECT 1 FROM "RoleAssignments"
    WHERE "SourceResourceId" = 'dda2e846-de85-4739-ba0c-ec15f63e48c7'
      AND "TargetResourceId" = '36ba74cb-c0a1-40a3-b224-e3be1c1b4b46'
      AND "ManagedIdentityType" = 'UserAssigned'
      AND "RoleDefinitionId" = '7f951dda-4ed3-4680-a7ca-43fe172d538e'
      AND "UserAssignedIdentityId" = '90097d4d-74a6-4d23-b94a-0392c57f7d14');

INSERT INTO "AppSettings" ("Id", "ResourceId", "Name")
SELECT gen_random_uuid(), '4615c4e9-1584-472d-b158-bdb41c49e4ed', 'AllowedHosts'
WHERE NOT EXISTS (SELECT 1 FROM "AppSettings" WHERE "ResourceId" = '4615c4e9-1584-472d-b158-bdb41c49e4ed' AND "Name" = 'AllowedHosts');

INSERT INTO "AppSettings" ("Id", "ResourceId", "Name", "SourceResourceId", "SourceOutputName")
SELECT gen_random_uuid(), '4615c4e9-1584-472d-b158-bdb41c49e4ed', 'APPLICATIONINSIGHTS_CONNECTION_STRING', 'e8bdb228-bc60-4de6-9d88-4d549b5a64bb', 'connectionString'
WHERE NOT EXISTS (SELECT 1 FROM "AppSettings" WHERE "ResourceId" = '4615c4e9-1584-472d-b158-bdb41c49e4ed' AND "Name" = 'APPLICATIONINSIGHTS_CONNECTION_STRING');

INSERT INTO "AppSettings" ("Id", "ResourceId", "Name")
SELECT gen_random_uuid(), '4615c4e9-1584-472d-b158-bdb41c49e4ed', setting_name
FROM (VALUES
    ('AzureAd__Audience'),
    ('AzureAd__ClientId'),
    ('AzureAd__Domain'),
    ('AzureAd__Instance'),
    ('AzureAd__TenantId'),
    ('BlobSettings__ContainerName'),
    ('JwtSettings__Audience'),
    ('JwtSettings__ExpiryMinutes'),
    ('JwtSettings__Issuer'),
    ('Logging__LogLevel__Default'),
    ('Logging__LogLevel__Microsoft.AspNetCore')
) AS s(setting_name)
WHERE NOT EXISTS (
    SELECT 1 FROM "AppSettings" a
    WHERE a."ResourceId" = '4615c4e9-1584-472d-b158-bdb41c49e4ed' AND a."Name" = s.setting_name);

INSERT INTO "AppSettings" ("Id", "ResourceId", "Name", "SourceResourceId", "SourceOutputName")
SELECT gen_random_uuid(), '4615c4e9-1584-472d-b158-bdb41c49e4ed', 'ConnectionStrings__AzureBlobStorageConnectionString', 'efe669ac-71a9-4884-a6b1-cf16583bbf37', 'connectionString'
WHERE NOT EXISTS (SELECT 1 FROM "AppSettings" WHERE "ResourceId" = '4615c4e9-1584-472d-b158-bdb41c49e4ed' AND "Name" = 'ConnectionStrings__AzureBlobStorageConnectionString');

INSERT INTO "AppSettings" ("Id", "ResourceId", "Name", "SourceResourceId", "SourceOutputName")
SELECT gen_random_uuid(), '4615c4e9-1584-472d-b158-bdb41c49e4ed', 'ConnectionStrings__infraDb', '9700666f-4771-46f9-aaab-74d9370eee59', 'connectionString'
WHERE NOT EXISTS (SELECT 1 FROM "AppSettings" WHERE "ResourceId" = '4615c4e9-1584-472d-b158-bdb41c49e4ed' AND "Name" = 'ConnectionStrings__infraDb');

INSERT INTO "AppSettings" ("Id", "ResourceId", "Name", "KeyVaultResourceId", "SecretName", "SecretValueAssignment", "PipelineVariableName")
SELECT gen_random_uuid(), '4615c4e9-1584-472d-b158-bdb41c49e4ed', 'JwtSettings__Secret', 'fc210d60-9d8a-4899-9f9e-07dced5871c5', 'jwt-secret', 'ViaBicepparam', 'jwt-secret'
WHERE NOT EXISTS (SELECT 1 FROM "AppSettings" WHERE "ResourceId" = '4615c4e9-1584-472d-b158-bdb41c49e4ed' AND "Name" = 'JwtSettings__Secret');

INSERT INTO "AppSettingEnvironmentValues" ("Id", "AppSettingId", "EnvironmentName", "Value")
SELECT gen_random_uuid(), a."Id", 'Development', v.setting_value
FROM "AppSettings" a
JOIN (VALUES
    ('AllowedHosts', '*'),
    ('AzureAd__Audience', ''),
    ('AzureAd__ClientId', ''),
    ('AzureAd__Domain', ''),
    ('AzureAd__Instance', ''),
    ('AzureAd__TenantId', ''),
    ('BlobSettings__ContainerName', 'bicep-output'),
    ('JwtSettings__Audience', 'InfraFlowSculptor'),
    ('JwtSettings__ExpiryMinutes', '120'),
    ('JwtSettings__Issuer', 'InfraFlowSculptor'),
    ('Logging__LogLevel__Default', 'azer'),
    ('Logging__LogLevel__Microsoft.AspNetCore', 'Warning')
) AS v(setting_name, setting_value)
  ON a."Name" = v.setting_name
WHERE a."ResourceId" = '4615c4e9-1584-472d-b158-bdb41c49e4ed'
  AND NOT EXISTS (
      SELECT 1 FROM "AppSettingEnvironmentValues" ev
      WHERE ev."AppSettingId" = a."Id" AND ev."EnvironmentName" = 'Development');

COMMIT;

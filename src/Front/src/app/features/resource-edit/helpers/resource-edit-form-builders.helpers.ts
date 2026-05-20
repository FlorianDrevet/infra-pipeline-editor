import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { AppConfigurationResponse } from '../../../shared/interfaces/app-configuration.interface';
import { AppServicePlanResponse } from '../../../shared/interfaces/app-service-plan.interface';
import { ApplicationInsightsResponse } from '../../../shared/interfaces/application-insights.interface';
import { EnvironmentDefinitionResponse } from '../../../shared/interfaces/infra-config.interface';
import { ContainerAppEnvironmentResponse } from '../../../shared/interfaces/container-app-environment.interface';
import { ContainerAppResponse } from '../../../shared/interfaces/container-app.interface';
import { ContainerRegistryResponse, AcrAuthMode } from '../../../shared/interfaces/container-registry.interface';
import { CosmosDbResponse } from '../../../shared/interfaces/cosmos-db.interface';
import { FunctionAppResponse } from '../../../shared/interfaces/function-app.interface';
import { KeyVaultResponse } from '../../../shared/interfaces/key-vault.interface';
import { LogAnalyticsWorkspaceResponse } from '../../../shared/interfaces/log-analytics-workspace.interface';
import { RedisCacheResponse } from '../../../shared/interfaces/redis-cache.interface';
import { ServiceBusNamespaceResponse } from '../../../shared/interfaces/service-bus-namespace.interface';
import { SqlDatabaseResponse } from '../../../shared/interfaces/sql-database.interface';
import { SqlServerResponse } from '../../../shared/interfaces/sql-server.interface';
import { StorageAccountResponse, BlobLifecycleRuleEntry, CorsRuleEntry } from '../../../shared/interfaces/storage-account.interface';
import { UserAssignedIdentityResponse } from '../../../shared/interfaces/user-assigned-identity.interface';
import { WebAppResponse } from '../../../shared/interfaces/web-app.interface';
import { buildBlobLifecycleRules, buildStorageAccountCorsRules, ResourceEditEnvironmentFormEntry } from './resource-edit-environment-settings.helpers';

export type ResourceEditData =
  | AppConfigurationResponse
  | AppServicePlanResponse
  | ApplicationInsightsResponse
  | ContainerAppEnvironmentResponse
  | ContainerAppResponse
  | ContainerRegistryResponse
  | CosmosDbResponse
  | FunctionAppResponse
  | KeyVaultResponse
  | LogAnalyticsWorkspaceResponse
  | RedisCacheResponse
  | ServiceBusNamespaceResponse
  | SqlDatabaseResponse
  | SqlServerResponse
  | StorageAccountResponse
  | UserAssignedIdentityResponse
  | WebAppResponse;

export interface ResourceEditGeneralFormBuildRequest {
  fb: FormBuilder;
  resourceType: string;
  resource: ResourceEditData;
  resolveAcrAuthMode: (containerRegistryId: string | null | undefined, acrAuthMode: AcrAuthMode | null | undefined) => AcrAuthMode | null;
}

export interface ResourceEditGeneralFormBuildResult {
  form: FormGroup;
  deploymentMode: 'Code' | 'Container';
  selectedContainerRegistryId: string | null;
  acrAuthMode: AcrAuthMode | null;
  storageCorsRulesDraft: CorsRuleEntry[];
  storageTableCorsRulesDraft: CorsRuleEntry[];
  lifecycleRulesDraft: BlobLifecycleRuleEntry[];
}

export function buildResourceEditGeneralForm(request: ResourceEditGeneralFormBuildRequest): ResourceEditGeneralFormBuildResult {
  const { fb, resource, resourceType, resolveAcrAuthMode } = request;
  const base: Record<string, unknown[]> = {
    name: [resource.name, [Validators.required, Validators.maxLength(80)]],
    location: [resource.location, [Validators.required]],
  };

  let deploymentMode: 'Code' | 'Container' = 'Code';
  let selectedContainerRegistryId: string | null = null;
  let acrAuthMode: AcrAuthMode | null = null;
  let storageCorsRulesDraft: CorsRuleEntry[] = [];
  let storageTableCorsRulesDraft: CorsRuleEntry[] = [];
  let lifecycleRulesDraft: BlobLifecycleRuleEntry[] = [];

  if (resourceType === 'KeyVault') {
    const keyVault = resource as KeyVaultResponse;
    base['enableRbacAuthorization'] = [keyVault.enableRbacAuthorization];
    base['enabledForDeployment'] = [keyVault.enabledForDeployment];
    base['enabledForDiskEncryption'] = [keyVault.enabledForDiskEncryption];
    base['enabledForTemplateDeployment'] = [keyVault.enabledForTemplateDeployment];
    base['enablePurgeProtection'] = [keyVault.enablePurgeProtection];
    base['enableSoftDelete'] = [keyVault.enableSoftDelete];
  } else if (resourceType === 'AppServicePlan') {
    const appServicePlan = resource as AppServicePlanResponse;
    base['osType'] = [appServicePlan.osType, [Validators.required]];
  } else if (resourceType === 'WebApp') {
    const webApp = resource as WebAppResponse;
    acrAuthMode = resolveAcrAuthMode(webApp.containerRegistryId ?? null, webApp.acrAuthMode ?? null);
    selectedContainerRegistryId = webApp.containerRegistryId ?? null;
    deploymentMode = (webApp.deploymentMode as 'Code' | 'Container') || 'Code';
    base['appServicePlanId'] = [webApp.appServicePlanId];
    base['deploymentMode'] = [deploymentMode];
    base['containerRegistryId'] = [selectedContainerRegistryId];
    base['acrAuthMode'] = [acrAuthMode];
    base['acrPullIdentityId'] = [webApp.acrPullIdentityId ?? null];
    base['dockerImageName'] = [webApp.dockerImageName ?? null];
    base['dockerImageValidated'] = [webApp.dockerImageValidated ?? false];
    base['runtimeStack'] = [webApp.runtimeStack, [Validators.required]];
    base['runtimeVersion'] = [webApp.runtimeVersion];
    base['alwaysOn'] = [webApp.alwaysOn];
    base['httpsOnly'] = [webApp.httpsOnly];
    base['dockerfilePath'] = [webApp.dockerfilePath ?? ''];
    base['sourceCodePath'] = [webApp.sourceCodePath ?? ''];
    base['buildCommand'] = [webApp.buildCommand ?? ''];
    base['applicationName'] = [webApp.applicationName ?? ''];
  } else if (resourceType === 'FunctionApp') {
    const functionApp = resource as FunctionAppResponse;
    acrAuthMode = resolveAcrAuthMode(functionApp.containerRegistryId ?? null, functionApp.acrAuthMode ?? null);
    selectedContainerRegistryId = functionApp.containerRegistryId ?? null;
    deploymentMode = (functionApp.deploymentMode as 'Code' | 'Container') || 'Code';
    base['appServicePlanId'] = [functionApp.appServicePlanId];
    base['deploymentMode'] = [deploymentMode];
    base['containerRegistryId'] = [selectedContainerRegistryId];
    base['acrAuthMode'] = [acrAuthMode];
    base['acrPullIdentityId'] = [functionApp.acrPullIdentityId ?? null];
    base['dockerImageName'] = [functionApp.dockerImageName ?? null];
    base['dockerImageValidated'] = [functionApp.dockerImageValidated ?? false];
    base['runtimeStack'] = [functionApp.runtimeStack, [Validators.required]];
    base['runtimeVersion'] = [functionApp.runtimeVersion];
    base['httpsOnly'] = [functionApp.httpsOnly];
    base['dockerfilePath'] = [functionApp.dockerfilePath ?? ''];
    base['sourceCodePath'] = [functionApp.sourceCodePath ?? ''];
    base['buildCommand'] = [functionApp.buildCommand ?? ''];
    base['applicationName'] = [functionApp.applicationName ?? ''];
  } else if (resourceType === 'StorageAccount') {
    const storageAccount = resource as StorageAccountResponse;
    base['kind'] = [storageAccount.kind, [Validators.required]];
    base['accessTier'] = [storageAccount.accessTier, [Validators.required]];
    base['allowBlobPublicAccess'] = [storageAccount.allowBlobPublicAccess];
    base['enableHttpsTrafficOnly'] = [storageAccount.enableHttpsTrafficOnly];
    base['minimumTlsVersion'] = [storageAccount.minimumTlsVersion, [Validators.required]];
    storageCorsRulesDraft = buildStorageAccountCorsRules(storageAccount.corsRules ?? []);
    storageTableCorsRulesDraft = buildStorageAccountCorsRules(storageAccount.tableCorsRules ?? []);
    lifecycleRulesDraft = buildBlobLifecycleRules(storageAccount.lifecycleRules ?? []);
  } else if (resourceType === 'ContainerApp') {
    const containerApp = resource as ContainerAppResponse;
    acrAuthMode = resolveAcrAuthMode(containerApp.containerRegistryId ?? null, containerApp.acrAuthMode ?? null);
    selectedContainerRegistryId = containerApp.containerRegistryId ?? null;
    base['containerAppEnvironmentId'] = [containerApp.containerAppEnvironmentId];
    base['containerRegistryId'] = [selectedContainerRegistryId];
    base['acrAuthMode'] = [acrAuthMode];
    base['acrPullIdentityId'] = [containerApp.acrPullIdentityId ?? null];
    base['dockerImageName'] = [containerApp.dockerImageName ?? null];
    base['dockerImageValidated'] = [containerApp.dockerImageValidated ?? false];
    base['dockerfilePath'] = [containerApp.dockerfilePath ?? ''];
    base['sourceCodePath'] = [containerApp.sourceCodePath ?? ''];
    base['applicationName'] = [containerApp.applicationName ?? ''];
  } else if (resourceType === 'ContainerAppEnvironment') {
    const containerAppEnvironment = resource as ContainerAppEnvironmentResponse;
    base['logAnalyticsWorkspaceId'] = [containerAppEnvironment.logAnalyticsWorkspaceId ?? null];
  } else if (resourceType === 'ApplicationInsights') {
    const applicationInsights = resource as ApplicationInsightsResponse;
    base['logAnalyticsWorkspaceId'] = [applicationInsights.logAnalyticsWorkspaceId];
  } else if (resourceType === 'RedisCache') {
    const redisCache = resource as RedisCacheResponse;
    base['redisVersion'] = [redisCache.redisVersion, [Validators.required]];
    base['enableNonSslPort'] = [redisCache.enableNonSslPort];
    base['minimumTlsVersion'] = [redisCache.minimumTlsVersion, [Validators.required]];
    base['disableAccessKeyAuthentication'] = [redisCache.disableAccessKeyAuthentication];
    base['enableAadAuth'] = [redisCache.enableAadAuth];
  } else if (resourceType === 'SqlServer') {
    const sqlServer = resource as SqlServerResponse;
    base['version'] = [sqlServer.version, [Validators.required]];
    base['administratorLogin'] = [sqlServer.administratorLogin, [Validators.required]];
  } else if (resourceType === 'SqlDatabase') {
    const sqlDatabase = resource as SqlDatabaseResponse;
    base['sqlServerId'] = [sqlDatabase.sqlServerId, [Validators.required]];
    base['collation'] = [sqlDatabase.collation];
  }

  return {
    form: fb.group(base),
    deploymentMode,
    selectedContainerRegistryId,
    acrAuthMode,
    storageCorsRulesDraft,
    storageTableCorsRulesDraft,
    lifecycleRulesDraft,
  };
}

export function buildResourceEditEnvironmentForms(
  fb: FormBuilder,
  resourceType: string,
  resource: ResourceEditData,
  environments: ReadonlyArray<EnvironmentDefinitionResponse>,
): ResourceEditEnvironmentFormEntry[] {
  return environments.map((environment) => ({
    envName: environment.name,
    form: buildSingleEnvironmentForm(fb, resourceType, resource, environment.name),
  }));
}

function buildSingleEnvironmentForm(
  fb: FormBuilder,
  resourceType: string,
  resource: ResourceEditData,
  environmentName: string,
): FormGroup {
  switch (resourceType) {
    case 'KeyVault': {
      const keyVault = resource as KeyVaultResponse;
      const settings = keyVault.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({ sku: [settings?.sku ?? null] });
    }
    case 'RedisCache': {
      const redisCache = resource as RedisCacheResponse;
      const settings = redisCache.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({
        sku: [settings?.sku ?? null],
        capacity: [settings?.capacity ?? null],
        maxMemoryPolicy: [settings?.maxMemoryPolicy ?? null],
      });
    }
    case 'StorageAccount': {
      const storageAccount = resource as StorageAccountResponse;
      const settings = storageAccount.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({ sku: [settings?.sku ?? null] });
    }
    case 'AppServicePlan': {
      const appServicePlan = resource as AppServicePlanResponse;
      const settings = appServicePlan.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({
        sku: [settings?.sku ?? null],
        capacity: [settings?.capacity ?? null],
      });
    }
    case 'WebApp': {
      const webApp = resource as WebAppResponse;
      const settings = webApp.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({
        alwaysOn: [settings?.alwaysOn ?? null],
        httpsOnly: [settings?.httpsOnly ?? null],
        dockerImageTag: [settings?.dockerImageTag ?? null],
      });
    }
    case 'FunctionApp': {
      const functionApp = resource as FunctionAppResponse;
      const settings = functionApp.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({
        httpsOnly: [settings?.httpsOnly ?? null],
        maxInstanceCount: [settings?.maxInstanceCount ?? null],
        dockerImageTag: [settings?.dockerImageTag ?? null],
      });
    }
    case 'AppConfiguration': {
      const appConfiguration = resource as AppConfigurationResponse;
      const settings = appConfiguration.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({
        sku: [settings?.sku ?? null],
        softDeleteRetentionInDays: [settings?.softDeleteRetentionInDays ?? null],
        purgeProtectionEnabled: [settings?.purgeProtectionEnabled ?? null],
        disableLocalAuth: [settings?.disableLocalAuth ?? null],
        publicNetworkAccess: [settings?.publicNetworkAccess ?? null],
      });
    }
    case 'ContainerAppEnvironment': {
      const containerAppEnvironment = resource as ContainerAppEnvironmentResponse;
      const settings = containerAppEnvironment.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({
        sku: [settings?.sku ?? null],
        workloadProfileType: [settings?.workloadProfileType ?? null],
        internalLoadBalancerEnabled: [settings?.internalLoadBalancerEnabled ?? null],
        zoneRedundancyEnabled: [settings?.zoneRedundancyEnabled ?? null],
      });
    }
    case 'ContainerApp': {
      const containerApp = resource as ContainerAppResponse;
      const settings = containerApp.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({
        cpuCores: [settings?.cpuCores ?? null],
        memoryGi: [settings?.memoryGi ?? null],
        minReplicas: [settings?.minReplicas ?? null],
        maxReplicas: [settings?.maxReplicas ?? null],
        ingressEnabled: [settings?.ingressEnabled ?? null],
        ingressTargetPort: [settings?.ingressTargetPort ?? null],
        ingressExternal: [settings?.ingressExternal ?? null],
        transportMethod: [settings?.transportMethod ?? null],
        readinessProbeEnabled: [Boolean(settings?.readinessProbePath)],
        readinessProbePath: [settings?.readinessProbePath ?? null],
        readinessProbePort: [settings?.readinessProbePort ?? null],
        livenessProbeEnabled: [Boolean(settings?.livenessProbePath)],
        livenessProbePath: [settings?.livenessProbePath ?? null],
        livenessProbePort: [settings?.livenessProbePort ?? null],
        startupProbeEnabled: [Boolean(settings?.startupProbePath)],
        startupProbePath: [settings?.startupProbePath ?? null],
        startupProbePort: [settings?.startupProbePort ?? null],
        containerRegistryServiceConnection: [settings?.containerRegistryServiceConnection ?? null],
      });
    }
    case 'LogAnalyticsWorkspace': {
      const logAnalyticsWorkspace = resource as LogAnalyticsWorkspaceResponse;
      const settings = logAnalyticsWorkspace.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({
        sku: [settings?.sku ?? null],
        retentionInDays: [settings?.retentionInDays ?? null],
        dailyQuotaGb: [settings?.dailyQuotaGb ?? null],
      });
    }
    case 'ApplicationInsights': {
      const applicationInsights = resource as ApplicationInsightsResponse;
      const settings = applicationInsights.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({
        samplingPercentage: [settings?.samplingPercentage ?? null],
        retentionInDays: [settings?.retentionInDays ?? null],
        disableIpMasking: [settings?.disableIpMasking ?? null],
        disableLocalAuth: [settings?.disableLocalAuth ?? null],
        ingestionMode: [settings?.ingestionMode ?? null],
      });
    }
    case 'CosmosDb': {
      const cosmosDb = resource as CosmosDbResponse;
      const settings = cosmosDb.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({
        databaseApiType: [settings?.databaseApiType ?? null],
        consistencyLevel: [settings?.consistencyLevel ?? null],
        maxStalenessPrefix: [settings?.maxStalenessPrefix ?? null],
        maxIntervalInSeconds: [settings?.maxIntervalInSeconds ?? null],
        enableAutomaticFailover: [settings?.enableAutomaticFailover ?? null],
        enableMultipleWriteLocations: [settings?.enableMultipleWriteLocations ?? null],
        backupPolicyType: [settings?.backupPolicyType ?? null],
        enableFreeTier: [settings?.enableFreeTier ?? null],
      });
    }
    case 'ServiceBusNamespace': {
      const serviceBusNamespace = resource as ServiceBusNamespaceResponse;
      const settings = serviceBusNamespace.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({
        sku: [settings?.sku ?? null],
        capacity: [settings?.capacity ?? null],
        zoneRedundant: [settings?.zoneRedundant ?? null],
        disableLocalAuth: [settings?.disableLocalAuth ?? null],
        minimumTlsVersion: [settings?.minimumTlsVersion ?? null],
      });
    }
    case 'ContainerRegistry': {
      const containerRegistry = resource as ContainerRegistryResponse;
      const settings = containerRegistry.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({
        sku: [settings?.sku ?? null],
        adminUserEnabled: [settings?.adminUserEnabled ?? null],
        publicNetworkAccess: [settings?.publicNetworkAccess ?? null],
        zoneRedundancy: [settings?.zoneRedundancy ?? null],
      });
    }
    case 'SqlServer': {
      const sqlServer = resource as SqlServerResponse;
      const settings = sqlServer.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({ minimalTlsVersion: [settings?.minimalTlsVersion ?? null] });
    }
    case 'SqlDatabase': {
      const sqlDatabase = resource as SqlDatabaseResponse;
      const settings = sqlDatabase.environmentSettings?.find((entry) => entry.environmentName === environmentName);
      return fb.group({
        sku: [settings?.sku ?? null],
        maxSizeGb: [settings?.maxSizeGb ?? null],
        zoneRedundant: [settings?.zoneRedundant ?? null],
      });
    }
    default:
      return fb.group({});
  }
}
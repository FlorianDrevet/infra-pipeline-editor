import { FormArray, FormBuilder, FormGroup } from '@angular/forms';

import { ResourceTypeEnum } from '../enums/resource-type.enum';
import { KeyVaultEnvironmentConfigEntry } from '../../../shared/interfaces/key-vault.interface';
import { RedisCacheEnvironmentConfigEntry } from '../../../shared/interfaces/redis-cache.interface';
import { StorageAccountEnvironmentConfigEntry } from '../../../shared/interfaces/storage-account.interface';
import { AppServicePlanEnvironmentConfigEntry } from '../../../shared/interfaces/app-service-plan.interface';
import { WebAppEnvironmentConfigEntry } from '../../../shared/interfaces/web-app.interface';
import { FunctionAppEnvironmentConfigEntry } from '../../../shared/interfaces/function-app.interface';
import { AppConfigurationEnvironmentConfigEntry } from '../../../shared/interfaces/app-configuration.interface';
import { ContainerAppEnvironmentEnvironmentConfigEntry } from '../../../shared/interfaces/container-app-environment.interface';
import { ContainerAppEnvironmentConfigEntry } from '../../../shared/interfaces/container-app.interface';
import { LogAnalyticsWorkspaceEnvironmentConfigEntry } from '../../../shared/interfaces/log-analytics-workspace.interface';
import { ApplicationInsightsEnvironmentConfigEntry } from '../../../shared/interfaces/application-insights.interface';
import { CosmosDbEnvironmentConfigEntry } from '../../../shared/interfaces/cosmos-db.interface';
import { SqlServerEnvironmentConfigEntry } from '../../../shared/interfaces/sql-server.interface';
import { SqlDatabaseEnvironmentConfigEntry } from '../../../shared/interfaces/sql-database.interface';
import { ServiceBusNamespaceEnvironmentConfigEntry } from '../../../shared/interfaces/service-bus-namespace.interface';
import { ContainerRegistryEnvironmentConfigEntry } from '../../../shared/interfaces/container-registry.interface';

export type AddResourceProbeType = 'readiness' | 'liveness' | 'startup';

export interface AddResourceEnvironmentDefinition {
  readonly name: string;
}

export interface AddResourceEnvironmentSettingsContext {
  readonly environments: readonly AddResourceEnvironmentDefinition[];
  readonly envFormArray: FormArray<FormGroup>;
}

interface AddResourceEnvironmentFormValue {
  readonly sku?: string | null;
  readonly skuName?: string | null;
  readonly capacity?: number | string | null;
  readonly maxMemoryPolicy?: string | null;
  readonly alwaysOn?: boolean | null;
  readonly httpsOnly?: boolean | null;
  readonly dockerImageTag?: string | null;
  readonly maxInstanceCount?: number | string | null;
  readonly softDeleteRetentionInDays?: number | string | null;
  readonly purgeProtectionEnabled?: boolean | null;
  readonly disableLocalAuth?: boolean | null;
  readonly publicNetworkAccess?: string | null;
  readonly workloadProfileType?: string | null;
  readonly internalLoadBalancerEnabled?: boolean | null;
  readonly zoneRedundancyEnabled?: boolean | null;
  readonly cpuCores?: string | null;
  readonly memoryGi?: string | null;
  readonly minReplicas?: number | string | null;
  readonly maxReplicas?: number | string | null;
  readonly ingressEnabled?: boolean | null;
  readonly ingressTargetPort?: number | string | null;
  readonly ingressExternal?: boolean | null;
  readonly transportMethod?: string | null;
  readonly readinessProbePath?: string | null;
  readonly readinessProbePort?: number | string | null;
  readonly livenessProbePath?: string | null;
  readonly livenessProbePort?: number | string | null;
  readonly startupProbePath?: string | null;
  readonly startupProbePort?: number | string | null;
  readonly retentionInDays?: number | string | null;
  readonly dailyQuotaGb?: number | string | null;
  readonly samplingPercentage?: number | string | null;
  readonly disableIpMasking?: boolean | null;
  readonly ingestionMode?: string | null;
  readonly databaseApiType?: string | null;
  readonly consistencyLevel?: string | null;
  readonly maxStalenessPrefix?: number | string | null;
  readonly maxIntervalInSeconds?: number | string | null;
  readonly enableAutomaticFailover?: boolean | null;
  readonly enableMultipleWriteLocations?: boolean | null;
  readonly backupPolicyType?: string | null;
  readonly enableFreeTier?: boolean | null;
  readonly minimalTlsVersion?: string | null;
  readonly maxSizeGb?: number | string | null;
  readonly zoneRedundant?: boolean | null;
  readonly minimumTlsVersion?: string | null;
  readonly adminUserEnabled?: boolean | null;
  readonly zoneRedundancy?: boolean | null;
}

const CONTAINER_APP_PROBE_DEFAULTS: Readonly<Record<AddResourceProbeType, { path: string; port: number }>> = {
  readiness: { path: '/healthz/ready', port: 8080 },
  liveness: { path: '/healthz/live', port: 8080 },
  startup: { path: '/healthz/startup', port: 8080 },
};

export function createAddResourceEnvironmentFormGroup(fb: FormBuilder, type: ResourceTypeEnum): FormGroup {
  switch (type) {
    case ResourceTypeEnum.KeyVault:
      return fb.group({
        sku: ['Standard'],
      });
    case ResourceTypeEnum.RedisCache:
      return fb.group({
        skuName: ['Standard'],
        capacity: [1],
        maxMemoryPolicy: ['NoEviction'],
      });
    case ResourceTypeEnum.StorageAccount:
      return fb.group({
        sku: ['Standard_LRS'],
      });
    case ResourceTypeEnum.AppServicePlan:
      return fb.group({
        sku: ['B1'],
        capacity: [1],
      });
    case ResourceTypeEnum.WebApp:
      return fb.group({
        alwaysOn: [true],
        httpsOnly: [true],
      });
    case ResourceTypeEnum.FunctionApp:
      return fb.group({
        httpsOnly: [true],
        maxInstanceCount: [null as number | null],
      });
    case ResourceTypeEnum.UserAssignedIdentity:
      return fb.group({});
    case ResourceTypeEnum.AppConfiguration:
      return fb.group({
        sku: ['Standard'],
        softDeleteRetentionInDays: [7],
        purgeProtectionEnabled: [false],
        disableLocalAuth: [false],
        publicNetworkAccess: ['Enabled'],
      });
    case ResourceTypeEnum.ContainerAppEnvironment:
      return fb.group({
        sku: ['Consumption'],
        workloadProfileType: ['Consumption'],
        internalLoadBalancerEnabled: [false],
        zoneRedundancyEnabled: [false],
      });
    case ResourceTypeEnum.ContainerApp:
      return fb.group({
        cpuCores: ['0.25'],
        memoryGi: ['0.5Gi'],
        minReplicas: [0],
        maxReplicas: [10],
        ingressEnabled: [true],
        ingressTargetPort: [80],
        ingressExternal: [true],
        transportMethod: ['auto'],
        readinessProbeEnabled: [false],
        readinessProbePath: [null as string | null],
        readinessProbePort: [null as number | null],
        livenessProbeEnabled: [false],
        livenessProbePath: [null as string | null],
        livenessProbePort: [null as number | null],
        startupProbeEnabled: [false],
        startupProbePath: [null as string | null],
        startupProbePort: [null as number | null],
      });
    case ResourceTypeEnum.LogAnalyticsWorkspace:
      return fb.group({
        sku: ['PerGB2018'],
        retentionInDays: [30],
        dailyQuotaGb: [null as number | null],
      });
    case ResourceTypeEnum.ApplicationInsights:
      return fb.group({
        samplingPercentage: [100],
        retentionInDays: [90],
        disableIpMasking: [false],
        disableLocalAuth: [false],
        ingestionMode: ['LogAnalytics'],
      });
    case ResourceTypeEnum.CosmosDb:
      return fb.group({
        databaseApiType: ['SQL'],
        consistencyLevel: ['Session'],
        maxStalenessPrefix: [null as number | null],
        maxIntervalInSeconds: [null as number | null],
        enableAutomaticFailover: [false],
        enableMultipleWriteLocations: [false],
        backupPolicyType: ['Continuous'],
        enableFreeTier: [false],
      });
    case ResourceTypeEnum.SqlServer:
      return fb.group({
        minimalTlsVersion: ['1.2'],
      });
    case ResourceTypeEnum.SqlDatabase:
      return fb.group({
        sku: ['Basic'],
        maxSizeGb: [null as number | null],
        zoneRedundant: [false],
      });
    case ResourceTypeEnum.ServiceBusNamespace:
      return fb.group({
        sku: ['Standard'],
        capacity: [null as number | null],
        zoneRedundant: [false],
        disableLocalAuth: [false],
        minimumTlsVersion: ['1.2'],
      });
    case ResourceTypeEnum.ContainerRegistry:
      return fb.group({
        sku: ['Standard'],
        adminUserEnabled: [false],
        publicNetworkAccess: ['Enabled'],
        zoneRedundancy: [false],
      });
    default:
      return fb.group({});
  }
}

export function copyAddResourceEnvironmentSettings(envFormArray: FormArray<FormGroup>, sourceIndex: number, targetIndex: number): void {
  const sourceGroup = envFormArray.at(sourceIndex);
  const targetGroup = envFormArray.at(targetIndex);

  targetGroup.patchValue(sourceGroup.getRawValue());
}

export function applyAddResourceProbeToggle(
  envFormArray: FormArray<FormGroup>,
  envIndex: number,
  probeType: AddResourceProbeType,
  enabled: boolean,
): void {
  const envGroup = envFormArray.at(envIndex);
  const defaults = CONTAINER_APP_PROBE_DEFAULTS[probeType];

  envGroup.patchValue({
    [`${probeType}ProbeEnabled`]: enabled,
    [`${probeType}ProbePath`]: enabled ? defaults.path : null,
    [`${probeType}ProbePort`]: enabled ? defaults.port : null,
  });
}

export function buildKeyVaultEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): KeyVaultEnvironmentConfigEntry[] {
  return buildSkuEnvironmentSettings<KeyVaultEnvironmentConfigEntry>(context);
}

export function buildRedisCacheEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): RedisCacheEnvironmentConfigEntry[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    sku: asStringOrNull(raw.skuName),
    capacity: asNumberOrNull(raw.capacity),
    maxMemoryPolicy: asStringOrNull(raw.maxMemoryPolicy),
  }));
}

export function buildStorageAccountEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): StorageAccountEnvironmentConfigEntry[] {
  return buildSkuEnvironmentSettings<StorageAccountEnvironmentConfigEntry>(context);
}

export function buildAppServicePlanEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): AppServicePlanEnvironmentConfigEntry[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    sku: asStringOrNull(raw.sku),
    capacity: asNumberOrNull(raw.capacity),
  }));
}

export function buildWebAppEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): WebAppEnvironmentConfigEntry[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    alwaysOn: asBooleanOrNull(raw.alwaysOn),
    httpsOnly: asBooleanOrNull(raw.httpsOnly),
    dockerImageTag: asStringOrNull(raw.dockerImageTag),
  }));
}

export function buildFunctionAppEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): FunctionAppEnvironmentConfigEntry[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    httpsOnly: asBooleanOrNull(raw.httpsOnly),
    maxInstanceCount: asNumberOrNull(raw.maxInstanceCount),
    dockerImageTag: asStringOrNull(raw.dockerImageTag),
  }));
}

export function buildAppConfigurationEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): AppConfigurationEnvironmentConfigEntry[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    sku: asStringOrNull(raw.sku),
    softDeleteRetentionInDays: asNumberOrNull(raw.softDeleteRetentionInDays),
    purgeProtectionEnabled: asBooleanOrNull(raw.purgeProtectionEnabled),
    disableLocalAuth: asBooleanOrNull(raw.disableLocalAuth),
    publicNetworkAccess: asStringOrNull(raw.publicNetworkAccess),
  }));
}

export function buildContainerAppEnvironmentEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): ContainerAppEnvironmentEnvironmentConfigEntry[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    sku: asStringOrNull(raw.sku),
    workloadProfileType: asStringOrNull(raw.workloadProfileType),
    internalLoadBalancerEnabled: asBooleanOrNull(raw.internalLoadBalancerEnabled),
    zoneRedundancyEnabled: asBooleanOrNull(raw.zoneRedundancyEnabled),
  }));
}

export function buildContainerAppEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): ContainerAppEnvironmentConfigEntry[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    cpuCores: asStringOrNull(raw.cpuCores),
    memoryGi: asStringOrNull(raw.memoryGi),
    minReplicas: asNumberOrNull(raw.minReplicas),
    maxReplicas: asNumberOrNull(raw.maxReplicas),
    ingressEnabled: asBooleanOrNull(raw.ingressEnabled),
    ingressTargetPort: asNumberOrNull(raw.ingressTargetPort),
    ingressExternal: asBooleanOrNull(raw.ingressExternal),
    transportMethod: asStringOrNull(raw.transportMethod),
    readinessProbePath: asStringOrNull(raw.readinessProbePath),
    readinessProbePort: asNumberOrNull(raw.readinessProbePort),
    livenessProbePath: asStringOrNull(raw.livenessProbePath),
    livenessProbePort: asNumberOrNull(raw.livenessProbePort),
    startupProbePath: asStringOrNull(raw.startupProbePath),
    startupProbePort: asNumberOrNull(raw.startupProbePort),
  }));
}

export function buildLogAnalyticsWorkspaceEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): LogAnalyticsWorkspaceEnvironmentConfigEntry[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    sku: asStringOrNull(raw.sku),
    retentionInDays: asNumberOrNull(raw.retentionInDays),
    dailyQuotaGb: asNumberOrNull(raw.dailyQuotaGb),
  }));
}

export function buildApplicationInsightsEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): ApplicationInsightsEnvironmentConfigEntry[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    samplingPercentage: asNumberOrNull(raw.samplingPercentage),
    retentionInDays: asNumberOrNull(raw.retentionInDays),
    disableIpMasking: asBooleanOrNull(raw.disableIpMasking),
    disableLocalAuth: asBooleanOrNull(raw.disableLocalAuth),
    ingestionMode: asStringOrNull(raw.ingestionMode),
  }));
}

export function buildCosmosDbEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): CosmosDbEnvironmentConfigEntry[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    databaseApiType: asStringOrNull(raw.databaseApiType),
    consistencyLevel: asStringOrNull(raw.consistencyLevel),
    maxStalenessPrefix: asNumberOrNull(raw.maxStalenessPrefix),
    maxIntervalInSeconds: asNumberOrNull(raw.maxIntervalInSeconds),
    enableAutomaticFailover: asBooleanOrNull(raw.enableAutomaticFailover),
    enableMultipleWriteLocations: asBooleanOrNull(raw.enableMultipleWriteLocations),
    backupPolicyType: asStringOrNull(raw.backupPolicyType),
    enableFreeTier: asBooleanOrNull(raw.enableFreeTier),
  }));
}

export function buildSqlServerEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): SqlServerEnvironmentConfigEntry[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    minimalTlsVersion: asStringOrNull(raw.minimalTlsVersion),
  }));
}

export function buildSqlDatabaseEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): SqlDatabaseEnvironmentConfigEntry[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    sku: asStringOrNull(raw.sku),
    maxSizeGb: asNumberOrNull(raw.maxSizeGb),
    zoneRedundant: asBooleanOrNull(raw.zoneRedundant),
  }));
}

export function buildServiceBusNamespaceEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): ServiceBusNamespaceEnvironmentConfigEntry[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    sku: asStringOrNull(raw.sku),
    capacity: asNumberOrNull(raw.capacity),
    zoneRedundant: asBooleanOrNull(raw.zoneRedundant),
    disableLocalAuth: asBooleanOrNull(raw.disableLocalAuth),
    minimumTlsVersion: asStringOrNull(raw.minimumTlsVersion),
  }));
}

export function buildContainerRegistryEnvironmentSettings(context: AddResourceEnvironmentSettingsContext): ContainerRegistryEnvironmentConfigEntry[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    sku: asStringOrNull(raw.sku),
    adminUserEnabled: asBooleanOrNull(raw.adminUserEnabled),
    publicNetworkAccess: asStringOrNull(raw.publicNetworkAccess),
    zoneRedundancy: asBooleanOrNull(raw.zoneRedundancy),
  }));
}

function buildEnvironmentSettings<T>(
  context: AddResourceEnvironmentSettingsContext,
  mapEntry: (environmentName: string, raw: AddResourceEnvironmentFormValue) => T,
): T[] {
  return context.environments.map((environment, index) => mapEntry(environment.name, getEnvironmentRawValue(context.envFormArray, index)));
}

function buildSkuEnvironmentSettings<T extends { environmentName: string; sku?: string | null }>(
  context: AddResourceEnvironmentSettingsContext,
): T[] {
  return buildEnvironmentSettings(context, (environmentName, raw) => ({
    environmentName,
    sku: asStringOrNull(raw.sku),
  } as T));
}

function getEnvironmentRawValue(envFormArray: FormArray<FormGroup>, index: number): AddResourceEnvironmentFormValue {
  return envFormArray.at(index).getRawValue() as AddResourceEnvironmentFormValue;
}

function asStringOrNull(value: string | number | boolean | null | undefined): string | null {
  return typeof value === 'string' && value.length > 0
    ? value
    : null;
}

function asNumberOrNull(value: string | number | boolean | null | undefined): number | null {
  if (value === null || value === undefined || value === '') {
    return null;
  }

  return Number(value);
}

function asBooleanOrNull(value: string | number | boolean | null | undefined): boolean | null {
  return typeof value === 'boolean'
    ? value
    : null;
}
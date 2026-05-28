import { FormGroup } from '@angular/forms';

import { AppConfigurationEnvironmentConfigEntry } from '../../../shared/interfaces/app-configuration.interface';
import { AppServicePlanEnvironmentConfigEntry } from '../../../shared/interfaces/app-service-plan.interface';
import { ApplicationInsightsEnvironmentConfigEntry } from '../../../shared/interfaces/application-insights.interface';
import { ContainerAppEnvironmentEnvironmentConfigEntry } from '../../../shared/interfaces/container-app-environment.interface';
import { ContainerAppEnvironmentConfigEntry } from '../../../shared/interfaces/container-app.interface';
import { ContainerRegistryEnvironmentConfigEntry } from '../../../shared/interfaces/container-registry.interface';
import { CosmosDbEnvironmentConfigEntry } from '../../../shared/interfaces/cosmos-db.interface';
import { FunctionAppEnvironmentConfigEntry } from '../../../shared/interfaces/function-app.interface';
import { KeyVaultEnvironmentConfigEntry } from '../../../shared/interfaces/key-vault.interface';
import { LogAnalyticsWorkspaceEnvironmentConfigEntry } from '../../../shared/interfaces/log-analytics-workspace.interface';
import { RedisCacheEnvironmentConfigEntry } from '../../../shared/interfaces/redis-cache.interface';
import { ServiceBusNamespaceEnvironmentConfigEntry } from '../../../shared/interfaces/service-bus-namespace.interface';
import { SqlDatabaseEnvironmentConfigEntry } from '../../../shared/interfaces/sql-database.interface';
import { SqlServerEnvironmentConfigEntry } from '../../../shared/interfaces/sql-server.interface';
import {
  BlobLifecycleRuleEntry,
  CorsRuleEntry,
  StorageAccountEnvironmentConfigEntry,
} from '../../../shared/interfaces/storage-account.interface';
import { VirtualNetworkEnvironmentConfigEntry } from '../../../shared/interfaces/virtual-network.interface';
import { WebAppEnvironmentConfigEntry } from '../../../shared/interfaces/web-app.interface';

export interface ResourceEditEnvironmentFormEntry {
  envName: string;
  form: FormGroup;
}

type RawEnvironmentScalarValue = string | number | boolean | null;

interface RawEnvironmentFormValue {
  sku?: RawEnvironmentScalarValue;
  capacity?: RawEnvironmentScalarValue;
  maxMemoryPolicy?: RawEnvironmentScalarValue;
  alwaysOn?: RawEnvironmentScalarValue;
  httpsOnly?: RawEnvironmentScalarValue;
  dockerImageTag?: RawEnvironmentScalarValue;
  maxInstanceCount?: RawEnvironmentScalarValue;
  softDeleteRetentionInDays?: RawEnvironmentScalarValue;
  purgeProtectionEnabled?: RawEnvironmentScalarValue;
  disableLocalAuth?: RawEnvironmentScalarValue;
  publicNetworkAccess?: RawEnvironmentScalarValue;
  workloadProfileType?: RawEnvironmentScalarValue;
  internalLoadBalancerEnabled?: RawEnvironmentScalarValue;
  zoneRedundancyEnabled?: RawEnvironmentScalarValue;
  cpuCores?: RawEnvironmentScalarValue;
  memoryGi?: RawEnvironmentScalarValue;
  minReplicas?: RawEnvironmentScalarValue;
  maxReplicas?: RawEnvironmentScalarValue;
  ingressEnabled?: RawEnvironmentScalarValue;
  ingressTargetPort?: RawEnvironmentScalarValue;
  ingressExternal?: RawEnvironmentScalarValue;
  transportMethod?: RawEnvironmentScalarValue;
  readinessProbePath?: RawEnvironmentScalarValue;
  readinessProbePort?: RawEnvironmentScalarValue;
  livenessProbePath?: RawEnvironmentScalarValue;
  livenessProbePort?: RawEnvironmentScalarValue;
  startupProbePath?: RawEnvironmentScalarValue;
  startupProbePort?: RawEnvironmentScalarValue;
  containerRegistryServiceConnection?: RawEnvironmentScalarValue;
  retentionInDays?: RawEnvironmentScalarValue;
  dailyQuotaGb?: RawEnvironmentScalarValue;
  samplingPercentage?: RawEnvironmentScalarValue;
  disableIpMasking?: RawEnvironmentScalarValue;
  ingestionMode?: RawEnvironmentScalarValue;
  databaseApiType?: RawEnvironmentScalarValue;
  consistencyLevel?: RawEnvironmentScalarValue;
  maxStalenessPrefix?: RawEnvironmentScalarValue;
  maxIntervalInSeconds?: RawEnvironmentScalarValue;
  enableAutomaticFailover?: RawEnvironmentScalarValue;
  enableMultipleWriteLocations?: RawEnvironmentScalarValue;
  backupPolicyType?: RawEnvironmentScalarValue;
  enableFreeTier?: RawEnvironmentScalarValue;
  zoneRedundant?: RawEnvironmentScalarValue;
  minimumTlsVersion?: RawEnvironmentScalarValue;
  adminUserEnabled?: RawEnvironmentScalarValue;
  zoneRedundancy?: RawEnvironmentScalarValue;
  minimalTlsVersion?: RawEnvironmentScalarValue;
  maxSizeGb?: RawEnvironmentScalarValue;
  addressSpacesInput?: readonly string[] | null;
  dnsServersInput?: readonly string[] | null;
}

export function buildKeyVaultEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): KeyVaultEnvironmentConfigEntry[] {
  return buildSkuEnvironmentSettings<KeyVaultEnvironmentConfigEntry>(envForms);
}

export function buildRedisCacheEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): RedisCacheEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      sku: toNullableString(raw.sku),
      capacity: toNullableNumber(raw.capacity),
      maxMemoryPolicy: toNullableString(raw.maxMemoryPolicy),
    };
  });
}

export function buildStorageAccountEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): StorageAccountEnvironmentConfigEntry[] {
  return buildSkuEnvironmentSettings<StorageAccountEnvironmentConfigEntry>(envForms);
}

export function buildVirtualNetworkEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): VirtualNetworkEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);
    const dnsServers = raw.dnsServersInput ?? [];

    return {
      environmentName: envForm.envName,
      addressSpaces: [...(raw.addressSpacesInput ?? [])],
      dnsServers: dnsServers.length > 0 ? [...dnsServers] : undefined,
    };
  });
}

export function buildStorageAccountCorsRules(rules: ReadonlyArray<CorsRuleEntry>): CorsRuleEntry[] {
  return rules.map((rule) => ({
    allowedOrigins: [...rule.allowedOrigins],
    allowedMethods: [...rule.allowedMethods],
    allowedHeaders: [...rule.allowedHeaders],
    exposedHeaders: [...rule.exposedHeaders],
    maxAgeInSeconds: rule.maxAgeInSeconds,
  }));
}

export function buildBlobLifecycleRules(rules: ReadonlyArray<BlobLifecycleRuleEntry>): BlobLifecycleRuleEntry[] {
  return rules.map((rule) => ({
    ruleName: rule.ruleName,
    containerNames: [...rule.containerNames],
    timeToLiveInDays: rule.timeToLiveInDays,
  }));
}

export function buildAppServicePlanEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): AppServicePlanEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      sku: toNullableString(raw.sku),
      capacity: toNullableNumber(raw.capacity),
    };
  });
}

export function buildWebAppEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): WebAppEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      alwaysOn: toNullableBoolean(raw.alwaysOn),
      httpsOnly: toNullableBoolean(raw.httpsOnly),
      dockerImageTag: toNullableString(raw.dockerImageTag),
    };
  });
}

export function buildFunctionAppEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): FunctionAppEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      httpsOnly: toNullableBoolean(raw.httpsOnly),
      maxInstanceCount: toNullableNumber(raw.maxInstanceCount),
      dockerImageTag: toNullableString(raw.dockerImageTag),
    };
  });
}

export function buildAppConfigurationEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): AppConfigurationEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      sku: toNullableString(raw.sku),
      softDeleteRetentionInDays: toNullableNumber(raw.softDeleteRetentionInDays),
      purgeProtectionEnabled: toNullableBoolean(raw.purgeProtectionEnabled),
      disableLocalAuth: toNullableBoolean(raw.disableLocalAuth),
      publicNetworkAccess: toNullableString(raw.publicNetworkAccess),
    };
  });
}

export function buildContainerAppEnvironmentResourceSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): ContainerAppEnvironmentEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      sku: toNullableString(raw.sku),
      workloadProfileType: toNullableString(raw.workloadProfileType),
      internalLoadBalancerEnabled: toNullableBoolean(raw.internalLoadBalancerEnabled),
      zoneRedundancyEnabled: toNullableBoolean(raw.zoneRedundancyEnabled),
    };
  });
}

export function buildContainerAppEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): ContainerAppEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      cpuCores: toNullableString(raw.cpuCores),
      memoryGi: toNullableString(raw.memoryGi),
      minReplicas: toNullableNumber(raw.minReplicas),
      maxReplicas: toNullableNumber(raw.maxReplicas),
      ingressEnabled: toNullableBoolean(raw.ingressEnabled),
      ingressTargetPort: toNullableNumber(raw.ingressTargetPort),
      ingressExternal: toNullableBoolean(raw.ingressExternal),
      transportMethod: toNullableString(raw.transportMethod),
      readinessProbePath: toNullableString(raw.readinessProbePath),
      readinessProbePort: toNullableNumber(raw.readinessProbePort),
      livenessProbePath: toNullableString(raw.livenessProbePath),
      livenessProbePort: toNullableNumber(raw.livenessProbePort),
      startupProbePath: toNullableString(raw.startupProbePath),
      startupProbePort: toNullableNumber(raw.startupProbePort),
      containerRegistryServiceConnection: toNullableString(raw.containerRegistryServiceConnection),
    };
  });
}

export function buildLogAnalyticsWorkspaceEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): LogAnalyticsWorkspaceEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      sku: toNullableString(raw.sku),
      retentionInDays: toNullableNumber(raw.retentionInDays),
      dailyQuotaGb: toNullableNumber(raw.dailyQuotaGb),
    };
  });
}

export function buildApplicationInsightsEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): ApplicationInsightsEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      samplingPercentage: toNullableNumber(raw.samplingPercentage),
      retentionInDays: toNullableNumber(raw.retentionInDays),
      disableIpMasking: toNullableBoolean(raw.disableIpMasking),
      disableLocalAuth: toNullableBoolean(raw.disableLocalAuth),
      ingestionMode: toNullableString(raw.ingestionMode),
    };
  });
}

export function buildCosmosDbEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): CosmosDbEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      databaseApiType: toNullableString(raw.databaseApiType),
      consistencyLevel: toNullableString(raw.consistencyLevel),
      maxStalenessPrefix: toNullableNumber(raw.maxStalenessPrefix),
      maxIntervalInSeconds: toNullableNumber(raw.maxIntervalInSeconds),
      enableAutomaticFailover: toNullableBoolean(raw.enableAutomaticFailover),
      enableMultipleWriteLocations: toNullableBoolean(raw.enableMultipleWriteLocations),
      backupPolicyType: toNullableString(raw.backupPolicyType),
      enableFreeTier: toNullableBoolean(raw.enableFreeTier),
    };
  });
}

export function buildServiceBusNamespaceEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): ServiceBusNamespaceEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      sku: toNullableString(raw.sku),
      capacity: toNullableNumber(raw.capacity),
      zoneRedundant: toNullableBoolean(raw.zoneRedundant),
      disableLocalAuth: toNullableBoolean(raw.disableLocalAuth),
      minimumTlsVersion: toNullableString(raw.minimumTlsVersion),
    };
  });
}

export function buildContainerRegistryEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): ContainerRegistryEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      sku: toNullableString(raw.sku),
      adminUserEnabled: toNullableBoolean(raw.adminUserEnabled),
      publicNetworkAccess: toNullableString(raw.publicNetworkAccess),
      zoneRedundancy: toNullableBoolean(raw.zoneRedundancy),
    };
  });
}

export function buildSqlServerEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): SqlServerEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      minimalTlsVersion: toNullableString(raw.minimalTlsVersion),
    };
  });
}

export function buildSqlDatabaseEnvironmentSettings(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): SqlDatabaseEnvironmentConfigEntry[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      sku: toNullableString(raw.sku),
      maxSizeGb: toNullableNumber(raw.maxSizeGb),
      zoneRedundant: toNullableBoolean(raw.zoneRedundant),
    };
  });
}

export function toNullableNumber(value: unknown): number | null {
  if (value === null || value === undefined || value === '') {
    return null;
  }

  const numericValue = Number(value);
  return Number.isNaN(numericValue) ? null : numericValue;
}

function buildSkuEnvironmentSettings<T extends { environmentName: string; sku?: string | null }>(
  envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>,
): T[] {
  return envForms.map((envForm) => {
    const raw = readRawValue(envForm);

    return {
      environmentName: envForm.envName,
      sku: toNullableString(raw.sku),
    } as T;
  });
}

function readRawValue(envForm: ResourceEditEnvironmentFormEntry): RawEnvironmentFormValue {
  return envForm.form.getRawValue() as RawEnvironmentFormValue;
}

function toNullableString(value: unknown): string | null {
  return typeof value === 'string' && value !== ''
    ? value
    : null;
}

function toNullableBoolean(value: unknown): boolean | null {
  return typeof value === 'boolean'
    ? value
    : null;
}
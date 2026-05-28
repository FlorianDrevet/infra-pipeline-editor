import { inject, Injectable } from '@angular/core';
import { FormArray, FormGroup } from '@angular/forms';

import { DsTagInputItem } from '../../../shared/components/ds/ds-tag-input/ds-tag-input.types';
import { ResourceTypeEnum } from '../enums/resource-type.enum';
import { AppConfigurationService } from '../../../shared/services/app-configuration.service';
import { AppServicePlanService } from '../../../shared/services/app-service-plan.service';
import { ApplicationInsightsService } from '../../../shared/services/application-insights.service';
import { ContainerAppEnvironmentService } from '../../../shared/services/container-app-environment.service';
import { ContainerAppService } from '../../../shared/services/container-app.service';
import { AcrAuthMode } from '../../../shared/interfaces/container-registry.interface';
import { ContainerRegistryService } from '../../../shared/services/container-registry.service';
import { CosmosDbService } from '../../../shared/services/cosmos-db.service';
import { FunctionAppService } from '../../../shared/services/function-app.service';
import { KeyVaultService } from '../../../shared/services/key-vault.service';
import { LogAnalyticsWorkspaceService } from '../../../shared/services/log-analytics-workspace.service';
import { RedisCacheService } from '../../../shared/services/redis-cache.service';
import { ServiceBusNamespaceService } from '../../../shared/services/service-bus-namespace.service';
import { SqlDatabaseService } from '../../../shared/services/sql-database.service';
import { SqlServerService } from '../../../shared/services/sql-server.service';
import { StorageAccountService } from '../../../shared/services/storage-account.service';
import { UserAssignedIdentityService } from '../../../shared/services/user-assigned-identity.service';
import { VirtualNetworkService } from '../../../shared/services/virtual-network.service';
import { WebAppService } from '../../../shared/services/web-app.service';
import {
  AddResourceEnvironmentDefinition,
  AddResourceEnvironmentSettingsContext,
  buildAppConfigurationEnvironmentSettings,
  buildAppServicePlanEnvironmentSettings,
  buildApplicationInsightsEnvironmentSettings,
  buildContainerAppEnvironmentEnvironmentSettings,
  buildContainerAppEnvironmentSettings,
  buildContainerRegistryEnvironmentSettings,
  buildCosmosDbEnvironmentSettings,
  buildFunctionAppEnvironmentSettings,
  buildKeyVaultEnvironmentSettings,
  buildLogAnalyticsWorkspaceEnvironmentSettings,
  buildRedisCacheEnvironmentSettings,
  buildServiceBusNamespaceEnvironmentSettings,
  buildSqlDatabaseEnvironmentSettings,
  buildSqlServerEnvironmentSettings,
  buildStorageAccountEnvironmentSettings,
  buildWebAppEnvironmentSettings,
} from './add-resource-dialog-environment-settings.helper';

interface AddResourceDialogCommonFormValue {
  readonly name: string;
  readonly location: string;
  readonly osType: string;
  readonly appServicePlanId: string;
  readonly containerAppEnvironmentId: string;
  readonly logAnalyticsWorkspaceId: string;
  readonly sqlServerId: string;
  readonly deploymentMode: 'Code' | 'Container';
  readonly containerRegistryId: string | null;
  readonly acrAuthMode: AcrAuthMode | null;
  readonly dockerImageName: string | null;
  readonly runtimeStack: string;
  readonly runtimeVersion: string;
  readonly alwaysOn: boolean;
  readonly httpsOnly: boolean;
  readonly version: string;
  readonly administratorLogin: string;
  readonly collation: string;
  readonly kind: string;
  readonly accessTier: string;
  readonly allowBlobPublicAccess: boolean;
  readonly enableHttpsTrafficOnly: boolean;
  readonly minimumTlsVersion: string;
  readonly redisVersion: number | null;
  readonly enableNonSslPort: boolean;
  readonly disableAccessKeyAuthentication: boolean;
  readonly enableAadAuth: boolean;
  readonly enableDdosProtection?: boolean;
  readonly vnetAddressSpacesInput: ReadonlyArray<DsTagInputItem>;
  readonly vnetDnsServersInput: ReadonlyArray<DsTagInputItem>;
  readonly isExisting: boolean;
}

function buildVirtualNetworkEnvironmentSettings(
  environments: readonly AddResourceEnvironmentDefinition[],
  common: AddResourceDialogCommonFormValue,
) {
  const addressSpaces = common.vnetAddressSpacesInput ?? [];
  if (addressSpaces.length === 0 || environments.length === 0) {
    return undefined;
  }

  const dnsServers = common.vnetDnsServersInput ?? [];

  return environments.map((environment) => ({
    environmentName: environment.name,
    addressSpaces: [...addressSpaces],
    dnsServers: dnsServers.length > 0 ? [...dnsServers] : undefined,
  }));
}

@Injectable()
export class AddResourceDialogResourceSubmitterService {
  private readonly appConfigurationService = inject(AppConfigurationService);
  private readonly appServicePlanService = inject(AppServicePlanService);
  private readonly applicationInsightsService = inject(ApplicationInsightsService);
  private readonly containerAppEnvironmentService = inject(ContainerAppEnvironmentService);
  private readonly containerAppService = inject(ContainerAppService);
  private readonly containerRegistryService = inject(ContainerRegistryService);
  private readonly cosmosDbService = inject(CosmosDbService);
  private readonly functionAppService = inject(FunctionAppService);
  private readonly keyVaultService = inject(KeyVaultService);
  private readonly logAnalyticsWorkspaceService = inject(LogAnalyticsWorkspaceService);
  private readonly redisCacheService = inject(RedisCacheService);
  private readonly serviceBusNamespaceService = inject(ServiceBusNamespaceService);
  private readonly sqlDatabaseService = inject(SqlDatabaseService);
  private readonly sqlServerService = inject(SqlServerService);
  private readonly storageAccountService = inject(StorageAccountService);
  private readonly userAssignedIdentityService = inject(UserAssignedIdentityService);
  private readonly virtualNetworkService = inject(VirtualNetworkService);
  private readonly webAppService = inject(WebAppService);

  async submit(command: {
    readonly type: ResourceTypeEnum;
    readonly common: AddResourceDialogCommonFormValue;
    readonly resourceGroupId: string;
    readonly environments: readonly AddResourceEnvironmentDefinition[];
    readonly envFormArray: FormArray<FormGroup>;
  }): Promise<void> {
    const { type, common, resourceGroupId, environments, envFormArray } = command;
    const environmentContext: AddResourceEnvironmentSettingsContext = { environments, envFormArray };
    const containerRegistryId = common.containerRegistryId || null;
    const dockerImageName = common.dockerImageName || null;
    const containerAcrAuthMode = this.resolveAcrAuthMode(containerRegistryId, common.acrAuthMode);
    const containerDeploymentMode = common.deploymentMode === 'Container';
    const conditionalAcrAuthMode = containerDeploymentMode ? containerAcrAuthMode : null;
    const conditionalContainerRegistryId = containerDeploymentMode ? containerRegistryId : null;
    const conditionalDockerImageName = containerDeploymentMode ? dockerImageName : null;

    switch (type) {
      case ResourceTypeEnum.KeyVault:
        await this.keyVaultService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          environmentSettings: buildKeyVaultEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.RedisCache:
        await this.redisCacheService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          redisVersion: common.redisVersion ? Number(common.redisVersion) : null,
          enableNonSslPort: common.enableNonSslPort,
          minimumTlsVersion: common.minimumTlsVersion || null,
          disableAccessKeyAuthentication: common.disableAccessKeyAuthentication,
          enableAadAuth: common.enableAadAuth,
          environmentSettings: buildRedisCacheEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.StorageAccount:
        await this.storageAccountService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          kind: common.kind,
          accessTier: common.accessTier,
          allowBlobPublicAccess: common.allowBlobPublicAccess,
          enableHttpsTrafficOnly: common.enableHttpsTrafficOnly,
          minimumTlsVersion: common.minimumTlsVersion,
          environmentSettings: buildStorageAccountEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.AppServicePlan:
        await this.appServicePlanService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          osType: common.osType,
          environmentSettings: buildAppServicePlanEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.WebApp:
        await this.webAppService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          appServicePlanId: common.appServicePlanId,
          deploymentMode: common.deploymentMode || 'Code',
          containerRegistryId: conditionalContainerRegistryId,
          acrAuthMode: conditionalAcrAuthMode,
          dockerImageName: conditionalDockerImageName,
          runtimeStack: common.runtimeStack,
          runtimeVersion: common.runtimeVersion,
          alwaysOn: common.alwaysOn,
          httpsOnly: common.httpsOnly,
          environmentSettings: buildWebAppEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.FunctionApp:
        await this.functionAppService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          appServicePlanId: common.appServicePlanId,
          deploymentMode: common.deploymentMode || 'Code',
          containerRegistryId: conditionalContainerRegistryId,
          acrAuthMode: conditionalAcrAuthMode,
          dockerImageName: conditionalDockerImageName,
          runtimeStack: common.runtimeStack,
          runtimeVersion: common.runtimeVersion,
          httpsOnly: common.httpsOnly,
          environmentSettings: buildFunctionAppEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.UserAssignedIdentity:
        await this.userAssignedIdentityService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.AppConfiguration:
        await this.appConfigurationService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          environmentSettings: buildAppConfigurationEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.ContainerAppEnvironment:
        await this.containerAppEnvironmentService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          logAnalyticsWorkspaceId: common.logAnalyticsWorkspaceId || null,
          environmentSettings: buildContainerAppEnvironmentEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.ContainerApp:
        await this.containerAppService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          containerAppEnvironmentId: common.containerAppEnvironmentId,
          containerRegistryId,
          acrAuthMode: containerAcrAuthMode,
          dockerImageName,
          environmentSettings: buildContainerAppEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.LogAnalyticsWorkspace:
        await this.logAnalyticsWorkspaceService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          environmentSettings: buildLogAnalyticsWorkspaceEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.ApplicationInsights:
        await this.applicationInsightsService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          logAnalyticsWorkspaceId: common.logAnalyticsWorkspaceId,
          environmentSettings: buildApplicationInsightsEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.CosmosDb:
        await this.cosmosDbService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          environmentSettings: buildCosmosDbEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.SqlServer:
        await this.sqlServerService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          version: common.version,
          administratorLogin: common.administratorLogin,
          environmentSettings: buildSqlServerEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.SqlDatabase:
        await this.sqlDatabaseService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          sqlServerId: common.sqlServerId,
          collation: common.collation,
          environmentSettings: buildSqlDatabaseEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.ServiceBusNamespace:
        await this.serviceBusNamespaceService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          environmentSettings: buildServiceBusNamespaceEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.ContainerRegistry:
        await this.containerRegistryService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          environmentSettings: buildContainerRegistryEnvironmentSettings(environmentContext),
          isExisting: common.isExisting,
        });
        return;
      case ResourceTypeEnum.VirtualNetwork:
        await this.virtualNetworkService.create({
          resourceGroupId,
          name: common.name,
          location: common.location,
          enableDdosProtection: common.enableDdosProtection ?? false,
          environmentSettings: buildVirtualNetworkEnvironmentSettings(environments, common),
          isExisting: common.isExisting,
        });
        return;
      default:
        throw new Error(`Unsupported resource type submission: ${type}`);
    }
  }

  private resolveAcrAuthMode(containerRegistryId: string | null | undefined, acrAuthMode: AcrAuthMode | null | undefined): AcrAuthMode | null {
    if (!containerRegistryId) {
      return null;
    }

    return acrAuthMode ?? 'ManagedIdentity';
  }
}
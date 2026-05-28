import { TestBed } from '@angular/core/testing';
import { FormArray, FormGroup } from '@angular/forms';

import { AppConfigurationService } from '../../../shared/services/app-configuration.service';
import { AppServicePlanService } from '../../../shared/services/app-service-plan.service';
import { ApplicationInsightsService } from '../../../shared/services/application-insights.service';
import { ContainerAppEnvironmentService } from '../../../shared/services/container-app-environment.service';
import { ContainerAppService } from '../../../shared/services/container-app.service';
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
import { VirtualNetworkResponse } from '../../../shared/interfaces/virtual-network.interface';
import { VirtualNetworkService } from '../../../shared/services/virtual-network.service';
import { WebAppService } from '../../../shared/services/web-app.service';
import { ResourceTypeEnum } from '../enums/resource-type.enum';
import { AddResourceDialogResourceSubmitterService } from './add-resource-dialog-resource-submitter.service';

type SubmitCommand = Parameters<AddResourceDialogResourceSubmitterService['submit']>[0];
type SubmitCommon = SubmitCommand['common'];

interface CreateServiceSpy {
  create: (...args: unknown[]) => Promise<unknown>;
}

const TEST_ADDRESS_SPACES = [
  `${buildIpv4(10, 0, 0, 0)}/16`,
  `${buildIpv4(10, 1, 0, 0)}/16`,
] as const;

const TEST_DNS_SERVERS = [
  buildIpv4(10, 0, 0, 4),
  buildIpv4(10, 0, 0, 5),
] as const;

describe('AddResourceDialogResourceSubmitterService', () => {
  let service: AddResourceDialogResourceSubmitterService;
  let virtualNetworkServiceSpy: jasmine.SpyObj<VirtualNetworkService>;

  beforeEach(() => {
    virtualNetworkServiceSpy = jasmine.createSpyObj<VirtualNetworkService>('VirtualNetworkService', ['create']);
    virtualNetworkServiceSpy.create.and.resolveTo(createVirtualNetworkResponse());

    TestBed.configureTestingModule({
      providers: [
        AddResourceDialogResourceSubmitterService,
        { provide: AppConfigurationService, useValue: createServiceSpy('AppConfigurationService') },
        { provide: AppServicePlanService, useValue: createServiceSpy('AppServicePlanService') },
        { provide: ApplicationInsightsService, useValue: createServiceSpy('ApplicationInsightsService') },
        { provide: ContainerAppEnvironmentService, useValue: createServiceSpy('ContainerAppEnvironmentService') },
        { provide: ContainerAppService, useValue: createServiceSpy('ContainerAppService') },
        { provide: ContainerRegistryService, useValue: createServiceSpy('ContainerRegistryService') },
        { provide: CosmosDbService, useValue: createServiceSpy('CosmosDbService') },
        { provide: FunctionAppService, useValue: createServiceSpy('FunctionAppService') },
        { provide: KeyVaultService, useValue: createServiceSpy('KeyVaultService') },
        { provide: LogAnalyticsWorkspaceService, useValue: createServiceSpy('LogAnalyticsWorkspaceService') },
        { provide: RedisCacheService, useValue: createServiceSpy('RedisCacheService') },
        { provide: ServiceBusNamespaceService, useValue: createServiceSpy('ServiceBusNamespaceService') },
        { provide: SqlDatabaseService, useValue: createServiceSpy('SqlDatabaseService') },
        { provide: SqlServerService, useValue: createServiceSpy('SqlServerService') },
        { provide: StorageAccountService, useValue: createServiceSpy('StorageAccountService') },
        { provide: UserAssignedIdentityService, useValue: createServiceSpy('UserAssignedIdentityService') },
        { provide: VirtualNetworkService, useValue: virtualNetworkServiceSpy },
        { provide: WebAppService, useValue: createServiceSpy('WebAppService') },
      ],
    });

    service = TestBed.inject(AddResourceDialogResourceSubmitterService);
  });

  it('calls VirtualNetworkService.create with the expected payload for virtual networks', async () => {
    await service.submit(createCommand(
      ResourceTypeEnum.VirtualNetwork,
      {
        name: 'demo-vnet',
        location: 'westeurope',
        enableDdosProtection: true,
        vnetAddressSpacesInput: [...TEST_ADDRESS_SPACES],
        vnetDnsServersInput: [...TEST_DNS_SERVERS],
        isExisting: true,
      },
      [
        { name: 'Development' },
        { name: 'Production' },
      ],
    ));

    expect(virtualNetworkServiceSpy.create).toHaveBeenCalledOnceWith({
      resourceGroupId: 'resource-group-1',
      name: 'demo-vnet',
      location: 'westeurope',
      enableDdosProtection: true,
      environmentSettings: [
        {
          environmentName: 'Development',
          addressSpaces: [...TEST_ADDRESS_SPACES],
          dnsServers: [...TEST_DNS_SERVERS],
        },
        {
          environmentName: 'Production',
          addressSpaces: [...TEST_ADDRESS_SPACES],
          dnsServers: [...TEST_DNS_SERVERS],
        },
      ],
      isExisting: true,
    });
  });

  it('rejects unsupported resource types instead of returning silently', async () => {
    const unsupportedType = 'UnsupportedResourceType' as ResourceTypeEnum;

    await expectAsync(service.submit(createCommand(unsupportedType))).toBeRejectedWithError(
      Error,
      'Unsupported resource type submission: UnsupportedResourceType',
    );
  });
});

function createServiceSpy(serviceName: string): jasmine.SpyObj<CreateServiceSpy> {
  return jasmine.createSpyObj<CreateServiceSpy>(serviceName, ['create']);
}

function createCommand(
  type: ResourceTypeEnum,
  commonOverrides: Partial<SubmitCommon> = {},
  environments: SubmitCommand['environments'] = [],
): SubmitCommand {
  return {
    type,
    resourceGroupId: 'resource-group-1',
    environments,
    envFormArray: new FormArray<FormGroup>([]),
    common: createCommonValue(commonOverrides),
  };
}

function createCommonValue(overrides: Partial<SubmitCommon> = {}): SubmitCommon {
  return {
    name: 'resource-name',
    location: 'francecentral',
    osType: 'Linux',
    appServicePlanId: '',
    containerAppEnvironmentId: '',
    logAnalyticsWorkspaceId: '',
    sqlServerId: '',
    deploymentMode: 'Code',
    containerRegistryId: null,
    acrAuthMode: null,
    dockerImageName: null,
    runtimeStack: '',
    runtimeVersion: '',
    alwaysOn: false,
    httpsOnly: false,
    version: '',
    administratorLogin: '',
    collation: '',
    kind: 'StorageV2',
    accessTier: 'Hot',
    allowBlobPublicAccess: false,
    enableHttpsTrafficOnly: true,
    minimumTlsVersion: 'Tls12',
    redisVersion: null,
    enableNonSslPort: false,
    disableAccessKeyAuthentication: false,
    enableAadAuth: false,
    enableDdosProtection: false,
    vnetAddressSpacesInput: [],
    vnetDnsServersInput: [],
    isExisting: false,
    ...overrides,
  };
}



function createVirtualNetworkResponse(): VirtualNetworkResponse {
  return {
    id: 'virtual-network-1',
    resourceGroupId: 'resource-group-1',
    name: 'demo-vnet',
    location: 'westeurope',
    enableDdosProtection: false,
    environmentSettings: [],
    subnets: [],
    isExisting: false,
  };
}

function buildIpv4(octet1: number, octet2: number, octet3: number, octet4: number): string {
  return [octet1, octet2, octet3, octet4].join('.');
}
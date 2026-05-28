import { FormBuilder } from '@angular/forms';

import { buildResourceEditEnvironmentForms, buildResourceEditGeneralForm } from './resource-edit-form-builders.helpers';

const TestAddressSpaces = ['vnet-space-primary', 'vnet-space-secondary'] as const;

const TestDnsServers = ['dns-server-primary', 'dns-server-secondary'] as const;

describe('resource edit form builders helpers', () => {
  it('builds a general storage account form and clones draft arrays', () => {
    const result = buildResourceEditGeneralForm({
      fb: new FormBuilder(),
      resourceType: 'StorageAccount',
      resource: createStorageAccountResource(),
      resolveAcrAuthMode: () => null,
    });

    expect(result.form.get('name')?.value).toBe('storage-main');
    expect(result.form.get('kind')?.value).toBe('StorageV2');
    expect(result.storageCorsRulesDraft[0]?.allowedOrigins).toEqual(['https://app.example.com']);
    expect(result.storageCorsRulesDraft[0]?.allowedOrigins).not.toBe(createStorageAccountResource().corsRules?.[0]?.allowedOrigins);
    expect(result.lifecycleRulesDraft[0]?.containerNames).toEqual(['assets']);
    expect(result.lifecycleRulesDraft[0]?.containerNames).not.toBe(createStorageAccountResource().lifecycleRules?.[0]?.containerNames);
  });

  it('builds container app environment forms with probe toggles derived from probe paths', () => {
    const forms = buildResourceEditEnvironmentForms(
      new FormBuilder(),
      'ContainerApp',
      createContainerAppResource(),
      [
        { id: 'env-dev', name: 'Development', shortName: 'dev', prefix: 'dev', suffix: 'svc', location: 'westeurope', subscriptionId: 'sub-1', order: 1, requiresApproval: false, azureResourceManagerConnection: null, tags: [] },
      ],
    );

    const form = forms[0].form;
    expect(form.get('ingressEnabled')?.value).toBeTrue();
    expect(form.get('readinessProbeEnabled')?.value).toBeTrue();
    expect(form.get('readinessProbePath')?.value).toBe('/ready');
    expect(form.get('startupProbeEnabled')?.value).toBeFalse();
  });

  it('builds container app environment forms with ACR service connection from environment settings', () => {
    const forms = buildResourceEditEnvironmentForms(
      new FormBuilder(),
      'ContainerApp',
      createContainerAppResource(),
      [
        { id: 'env-dev', name: 'Development', shortName: 'dev', prefix: 'dev', suffix: 'svc', location: 'westeurope', subscriptionId: 'sub-1', order: 1, requiresApproval: false, azureResourceManagerConnection: null, tags: [] },
      ],
    );

    expect(forms[0].form.get('containerRegistryServiceConnection')?.value).toBe('acr-dev-docker');
  });

  it('builds general container app form with acrPullIdentityId control from response', () => {
    const resource = {
      ...createContainerAppResource(),
      acrPullIdentityId: 'uai-123',
    };

    const result = buildResourceEditGeneralForm({
      fb: new FormBuilder(),
      resourceType: 'ContainerApp',
      resource,
      resolveAcrAuthMode: () => 'ManagedIdentity',
    });

    expect(result.form.get('acrPullIdentityId')?.value).toBe('uai-123');
  });

  it('builds virtual network general and environment forms with VNet-specific values', () => {
    const resource = createVirtualNetworkResource();

    const generalResult = buildResourceEditGeneralForm({
      fb: new FormBuilder(),
      resourceType: 'VirtualNetwork',
      resource,
      resolveAcrAuthMode: () => null,
    });

    const envForms = buildResourceEditEnvironmentForms(
      new FormBuilder(),
      'VirtualNetwork',
      resource,
      [
        { id: 'env-dev', name: 'Development', shortName: 'dev', prefix: 'dev', suffix: 'svc', location: 'westeurope', subscriptionId: 'sub-1', order: 1, requiresApproval: false, azureResourceManagerConnection: null, tags: [] },
        { id: 'env-prod', name: 'Production', shortName: 'prod', prefix: 'prod', suffix: 'svc', location: 'westeurope', subscriptionId: 'sub-1', order: 2, requiresApproval: false, azureResourceManagerConnection: null, tags: [] },
      ],
    );

    expect(generalResult.form.get('enableDdosProtection')?.value).toBeTrue();
    expect(envForms[0].form.get('addressSpacesInput')?.value).toEqual([...TestAddressSpaces]);
    expect(envForms[0].form.get('dnsServersInput')?.value).toEqual([...TestDnsServers]);
    expect(envForms[1].form.get('addressSpacesInput')?.value).toEqual([]);
    expect(envForms[1].form.get('dnsServersInput')?.value).toEqual([]);
  });
});

function createStorageAccountResource() {
  return {
    id: 'storage-1',
    name: 'storage-main',
    location: 'westeurope',
    resourceGroupId: 'rg-1',
    kind: 'StorageV2',
    accessTier: 'Hot',
    allowBlobPublicAccess: false,
    enableHttpsTrafficOnly: true,
    minimumTlsVersion: 'TLS1_2',
    blobContainers: [],
    queues: [],
    tables: [],
    corsRules: [
      {
        allowedOrigins: ['https://app.example.com'],
        allowedMethods: ['GET'],
        allowedHeaders: ['content-type'],
        exposedHeaders: ['etag'],
        maxAgeInSeconds: 3600,
      },
    ],
    tableCorsRules: [],
    lifecycleRules: [
      {
        ruleName: 'delete-old-assets',
        containerNames: ['assets'],
        timeToLiveInDays: 30,
      },
    ],
    environmentSettings: [],
  } as const;
}

function createContainerAppResource() {
  return {
    id: 'container-app-1',
    name: 'api',
    location: 'westeurope',
    resourceGroupId: 'rg-1',
    containerAppEnvironmentId: 'cae-1',
    containerRegistryId: 'acr-1',
    dockerImageName: 'api:latest',
    dockerfilePath: '',
    applicationName: 'api',
    acrAuthMode: 'ManagedIdentity',
    environmentSettings: [
      {
        environmentName: 'Development',
        cpuCores: '0.5',
        memoryGi: '1',
        minReplicas: 1,
        maxReplicas: 3,
        ingressEnabled: true,
        ingressTargetPort: 8080,
        ingressExternal: true,
        transportMethod: 'Auto',
        readinessProbePath: '/ready',
        readinessProbePort: 8080,
        livenessProbePath: '/live',
        livenessProbePort: 8080,
        startupProbePath: null,
        startupProbePort: null,
        containerRegistryServiceConnection: 'acr-dev-docker',
      },
    ],
  } as const;
}

function createVirtualNetworkResource() {
  return {
    id: 'virtual-network-1',
    name: 'demo-vnet',
    location: 'westeurope',
    resourceGroupId: 'rg-1',
    enableDdosProtection: true,
    environmentSettings: [
      {
        environmentName: 'Development',
        addressSpaces: [...TestAddressSpaces],
        dnsServers: [...TestDnsServers],
      },
      {
        environmentName: 'Production',
        addressSpaces: [],
        dnsServers: null,
      },
    ],
    subnets: [
      {
        id: 'subnet-1',
        name: 'snet-app',
        delegation: null,
        serviceEndpoints: [],
        privateEndpointNetworkPolicies: 'Enabled',
        nsgId: null,
      },
    ],
    isExisting: false,
  } as const;
}

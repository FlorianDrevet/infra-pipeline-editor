import { FormBuilder } from '@angular/forms';

import { BlobLifecycleRuleEntry, CorsRuleEntry } from '../../../shared/interfaces/storage-account.interface';
import {
  ResourceEditEnvironmentFormEntry,
  buildBlobLifecycleRules,
  buildContainerAppEnvironmentSettings,
  buildVirtualNetworkEnvironmentSettings,
  buildStorageAccountCorsRules,
} from './resource-edit-environment-settings.helpers';

const TestAddressSpaces = ['vnet-space-primary', 'vnet-space-secondary'] as const;

const TestDnsServers = ['dns-server-primary', 'dns-server-secondary'] as const;

describe('resource edit environment settings helpers', () => {
  const fb = new FormBuilder();

  it('builds virtual network environment settings from delimited address-space and DNS inputs', () => {
    const envForms = [
      createEnvironmentFormEntry('Development', {
        addressSpacesInput: [...TestAddressSpaces],
        dnsServersInput: [...TestDnsServers],
      }),
      createEnvironmentFormEntry('Production', {
        addressSpacesInput: [],
        dnsServersInput: [],
      }),
    ];

    expect(buildVirtualNetworkEnvironmentSettings(envForms)).toEqual([
      {
        environmentName: 'Development',
        addressSpaces: [...TestAddressSpaces],
        dnsServers: [...TestDnsServers],
      },
      {
        environmentName: 'Production',
        addressSpaces: [],
        dnsServers: undefined,
      },
    ]);
  });

  it('builds container app environment payloads with ACR service connection names', () => {
    const envForms = [
      createEnvironmentFormEntry('Development', {
        containerRegistryServiceConnection: 'acr-dev-docker',
      }),
      createEnvironmentFormEntry('Production', {
        containerRegistryServiceConnection: '',
      }),
    ];

    const payload = buildContainerAppEnvironmentSettings(envForms);

    expect(payload).toEqual([
      {
        environmentName: 'Development',
        cpuCores: null,
        memoryGi: null,
        minReplicas: null,
        maxReplicas: null,
        ingressEnabled: null,
        ingressTargetPort: null,
        ingressExternal: null,
        transportMethod: null,
        readinessProbePath: null,
        readinessProbePort: null,
        livenessProbePath: null,
        livenessProbePort: null,
        startupProbePath: null,
        startupProbePort: null,
        containerRegistryServiceConnection: 'acr-dev-docker',
      },
      {
        environmentName: 'Production',
        cpuCores: null,
        memoryGi: null,
        minReplicas: null,
        maxReplicas: null,
        ingressEnabled: null,
        ingressTargetPort: null,
        ingressExternal: null,
        transportMethod: null,
        readinessProbePath: null,
        readinessProbePort: null,
        livenessProbePath: null,
        livenessProbePort: null,
        startupProbePath: null,
        startupProbePort: null,
        containerRegistryServiceConnection: null,
      },
    ]);
  });

  it('builds container app environment payloads with null-safe numeric conversion', () => {
    const envForms = [
      createEnvironmentFormEntry('Development', {
        cpuCores: '0.5',
        memoryGi: '1.0Gi',
        minReplicas: '1',
        maxReplicas: '3',
        ingressEnabled: true,
        ingressTargetPort: '8080',
        ingressExternal: false,
        transportMethod: 'http',
        readinessProbePath: '/health/ready',
        readinessProbePort: '8081',
        livenessProbePath: '/health/live',
        livenessProbePort: '',
        startupProbePath: '',
        startupProbePort: null,
      }),
    ];

    expect(buildContainerAppEnvironmentSettings(envForms)).toEqual([
      {
        environmentName: 'Development',
        cpuCores: '0.5',
        memoryGi: '1.0Gi',
        minReplicas: 1,
        maxReplicas: 3,
        ingressEnabled: true,
        ingressTargetPort: 8080,
        ingressExternal: false,
        transportMethod: 'http',
        readinessProbePath: '/health/ready',
        readinessProbePort: 8081,
        livenessProbePath: '/health/live',
        livenessProbePort: null,
        startupProbePath: null,
        startupProbePort: null,
        containerRegistryServiceConnection: null,
      },
    ]);
  });

  it('clones storage drafts before emitting payload arrays', () => {
    const corsDrafts: CorsRuleEntry[] = [
      {
        allowedOrigins: ['https://api.example.com'],
        allowedMethods: ['GET'],
        allowedHeaders: ['x-ms-meta-*'],
        exposedHeaders: ['etag'],
        maxAgeInSeconds: 3600,
      },
    ];
    const lifecycleDrafts: BlobLifecycleRuleEntry[] = [
      {
        ruleName: 'expire-assets',
        containerNames: ['assets'],
        timeToLiveInDays: 30,
      },
    ];

    const corsPayload = buildStorageAccountCorsRules(corsDrafts);
    const lifecyclePayload = buildBlobLifecycleRules(lifecycleDrafts);

    corsDrafts[0].allowedOrigins.push('https://mutated.example.com');
    lifecycleDrafts[0].containerNames.push('logs');

    expect(corsPayload).toEqual([
      {
        allowedOrigins: ['https://api.example.com'],
        allowedMethods: ['GET'],
        allowedHeaders: ['x-ms-meta-*'],
        exposedHeaders: ['etag'],
        maxAgeInSeconds: 3600,
      },
    ]);
    expect(lifecyclePayload).toEqual([
      {
        ruleName: 'expire-assets',
        containerNames: ['assets'],
        timeToLiveInDays: 30,
      },
    ]);
  });

  function createEnvironmentFormEntry(
    envName: string,
    rawValue: Record<string, string | number | boolean | null | string[]>,
  ): ResourceEditEnvironmentFormEntry {
    const form = fb.group({});

    for (const [key, value] of Object.entries(rawValue)) {
      form.addControl(key, fb.control(value));
    }

    return {
      envName,
      form,
    };
  }
});



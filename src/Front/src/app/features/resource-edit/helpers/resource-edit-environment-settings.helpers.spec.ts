import { FormBuilder } from '@angular/forms';

import { DsTagInputItem } from '../../../shared/components/ds/ds-tag-input/ds-tag-input.types';
import { BlobLifecycleRuleEntry, CorsRuleEntry } from '../../../shared/interfaces/storage-account.interface';
import {
  ResourceEditEnvironmentFormEntry,
  buildBlobLifecycleRules,
  buildContainerAppEnvironmentSettings,
  buildVirtualNetworkEnvironmentSettings,
  buildStorageAccountCorsRules,
} from './resource-edit-environment-settings.helpers';

describe('resource edit environment settings helpers', () => {
  const fb = new FormBuilder();

  it('builds virtual network environment settings from delimited address-space and DNS inputs', () => {
    const envForms = [
      createEnvironmentFormEntry('Development', {
        addressSpacesInput: createTagItems(['10.0.0.0/16', '10.1.0.0/16']),
        dnsServersInput: createTagItems(['10.0.0.4', '10.0.0.5']),
      }),
      createEnvironmentFormEntry('Production', {
        addressSpacesInput: [],
        dnsServersInput: [],
      }),
    ];

    expect(buildVirtualNetworkEnvironmentSettings(envForms)).toEqual([
      {
        environmentName: 'Development',
        addressSpaces: ['10.0.0.0/16', '10.1.0.0/16'],
        dnsServers: ['10.0.0.4', '10.0.0.5'],
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
    rawValue: Record<string, string | number | boolean | null | DsTagInputItem[]>,
  ): ResourceEditEnvironmentFormEntry {
    return {
      envName,
      form: fb.group(rawValue),
    };
  }

  function createTagItems(values: readonly string[]): DsTagInputItem[] {
    return values.map((value) => ({ value }));
  }
});
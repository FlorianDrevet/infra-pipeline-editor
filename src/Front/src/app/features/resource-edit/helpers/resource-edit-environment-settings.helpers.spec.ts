import { FormBuilder } from '@angular/forms';

import { BlobLifecycleRuleEntry, CorsRuleEntry } from '../../../shared/interfaces/storage-account.interface';
import {
  ResourceEditEnvironmentFormEntry,
  buildBlobLifecycleRules,
  buildContainerAppEnvironmentSettings,
  buildStorageAccountCorsRules,
} from './resource-edit-environment-settings.helpers';

describe('resource edit environment settings helpers', () => {
  const fb = new FormBuilder();

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
    rawValue: Record<string, string | number | boolean | null>,
  ): ResourceEditEnvironmentFormEntry {
    return {
      envName,
      form: fb.group(rawValue),
    };
  }
});
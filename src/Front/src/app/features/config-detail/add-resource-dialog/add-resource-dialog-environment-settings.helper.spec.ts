import { FormArray, FormBuilder, FormGroup } from '@angular/forms';

import {
  applyAddResourceProbeToggle,
  buildRedisCacheEnvironmentSettings,
  copyAddResourceEnvironmentSettings,
  createAddResourceEnvironmentFormGroup,
} from './add-resource-dialog-environment-settings.helper';
import { ResourceTypeEnum } from '../enums/resource-type.enum';
import { hasResourceTypeEnvironmentSettings } from '../../../shared/resource-metadata/resource-type.metadata';

describe('add resource dialog environment settings helper', () => {
  it('treats only resource types with real per-environment fields as environment-aware', () => {
    expect(hasResourceTypeEnvironmentSettings(ResourceTypeEnum.RedisCache)).toBeTrue();
    expect(hasResourceTypeEnvironmentSettings(ResourceTypeEnum.ContainerApp)).toBeTrue();
    expect(hasResourceTypeEnvironmentSettings(ResourceTypeEnum.UserAssignedIdentity)).toBeFalse();
    expect(hasResourceTypeEnvironmentSettings(ResourceTypeEnum.EventHubNamespace)).toBeFalse();
    expect(hasResourceTypeEnvironmentSettings(ResourceTypeEnum.VirtualNetwork)).toBeTrue();
  });

  it('creates redis cache environment forms with the expected defaults', () => {
    const group = createAddResourceEnvironmentFormGroup(new FormBuilder(), ResourceTypeEnum.RedisCache);

    expect(group.getRawValue()).toEqual({
      skuName: 'Standard',
      capacity: 1,
      maxMemoryPolicy: 'NoEviction',
    });
  });

  it('copies environment settings from one environment form to another', () => {
    const envFormArray = new FormArray<FormGroup>([
      createAddResourceEnvironmentFormGroup(new FormBuilder(), ResourceTypeEnum.RedisCache),
      createAddResourceEnvironmentFormGroup(new FormBuilder(), ResourceTypeEnum.RedisCache),
    ]);

    envFormArray.at(0).patchValue({ skuName: 'Premium', capacity: 3, maxMemoryPolicy: 'AllKeysLru' });

    copyAddResourceEnvironmentSettings(envFormArray, 0, 1);

    expect(envFormArray.at(1).getRawValue()).toEqual({
      skuName: 'Premium',
      capacity: 3,
      maxMemoryPolicy: 'AllKeysLru',
    });
  });

  it('applies the default container app probe path and port when a probe is enabled', () => {
    const envFormArray = new FormArray<FormGroup>([
      createAddResourceEnvironmentFormGroup(new FormBuilder(), ResourceTypeEnum.ContainerApp),
    ]);

    applyAddResourceProbeToggle(envFormArray, 0, 'readiness', true);

    expect(envFormArray.at(0).getRawValue()).toEqual(jasmine.objectContaining({
      readinessProbeEnabled: true,
      readinessProbePath: '/healthz/ready',
      readinessProbePort: 8080,
    }));
  });

  it('builds redis cache environment settings from the form array values', () => {
    const envFormArray = new FormArray<FormGroup>([
      createAddResourceEnvironmentFormGroup(new FormBuilder(), ResourceTypeEnum.RedisCache),
      createAddResourceEnvironmentFormGroup(new FormBuilder(), ResourceTypeEnum.RedisCache),
    ]);

    envFormArray.at(0).patchValue({ skuName: 'Standard', capacity: 2, maxMemoryPolicy: 'AllKeysLru' });
    envFormArray.at(1).patchValue({ skuName: 'Premium', capacity: 4, maxMemoryPolicy: 'VolatileTtl' });

    const settings = buildRedisCacheEnvironmentSettings({
      environments: [{ name: 'dev' }, { name: 'prod' }],
      envFormArray,
    });

    expect(settings).toEqual([
      {
        environmentName: 'dev',
        sku: 'Standard',
        capacity: 2,
        maxMemoryPolicy: 'AllKeysLru',
      },
      {
        environmentName: 'prod',
        sku: 'Premium',
        capacity: 4,
        maxMemoryPolicy: 'VolatileTtl',
      },
    ]);
  });
});
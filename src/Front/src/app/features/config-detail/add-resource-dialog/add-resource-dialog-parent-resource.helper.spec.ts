import { FormBuilder } from '@angular/forms';

import {
  patchAddResourceParentResourceSelection,
  resolveAddResourceParentResourceDescriptor,
  resolveAllowedChildResourceTypes,
} from './add-resource-dialog-parent-resource.helper';
import { ResourceTypeEnum } from '../enums/resource-type.enum';

const APP_SERVICE_PLAN_CHILD_TYPES = [ResourceTypeEnum.WebApp, ResourceTypeEnum.FunctionApp];

describe('add resource dialog parent resource helper', () => {
  it('maps container app flows to container app environment parent metadata', () => {
    const descriptor = resolveAddResourceParentResourceDescriptor(ResourceTypeEnum.ContainerApp);

    expect(descriptor).toEqual(jasmine.objectContaining({
      suffix: 'CAE',
      icon: 'cloud_queue',
      filterType: 'ContainerAppEnvironment',
      formControlName: 'containerAppEnvironmentId',
      requiresPlanSelection: true,
      requiresContainerRegistryLoading: true,
    }));
  });

  it('maps sql database flows to sql server parent metadata', () => {
    const descriptor = resolveAddResourceParentResourceDescriptor(ResourceTypeEnum.SqlDatabase);

    expect(descriptor).toEqual(jasmine.objectContaining({
      suffix: 'SQL',
      icon: 'dns',
      filterType: 'SqlServer',
      formControlName: 'sqlServerId',
      requiresPlanSelection: true,
      requiresContainerRegistryLoading: false,
    }));
  });

  it('resolves allowed child types from the parent resource map', () => {
    const allowedChildTypes = resolveAllowedChildResourceTypes(ResourceTypeEnum.AppServicePlan);

    expect(allowedChildTypes).toEqual(APP_SERVICE_PLAN_CHILD_TYPES);
  });

  it('patches the matching parent control for the selected resource type', () => {
    const form = new FormBuilder().group({
      appServicePlanId: [''],
      containerAppEnvironmentId: [''],
      logAnalyticsWorkspaceId: [''],
      sqlServerId: [''],
    });

    patchAddResourceParentResourceSelection(form, ResourceTypeEnum.ApplicationInsights, 'law-123');

    expect(form.getRawValue()).toEqual({
      appServicePlanId: '',
      containerAppEnvironmentId: '',
      logAnalyticsWorkspaceId: 'law-123',
      sqlServerId: '',
    });
  });
});
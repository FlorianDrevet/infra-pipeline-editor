import { PARENT_CHILD_RESOURCE_TYPES, ResourceTypeEnum } from '../enums/resource-type.enum';

export type AddResourceParentResourceSuffix = 'ASP' | 'CAE' | 'LAW' | 'SQL';
export type AddResourceParentFormControlName = 'appServicePlanId' | 'containerAppEnvironmentId' | 'logAnalyticsWorkspaceId' | 'sqlServerId';

export interface AddResourceParentResourceDescriptor {
  readonly suffix: AddResourceParentResourceSuffix;
  readonly icon: string;
  readonly filterType: string;
  readonly formControlName: AddResourceParentFormControlName;
  readonly requiresPlanSelection: boolean;
  readonly requiresContainerRegistryLoading: boolean;
}

export interface AddResourceParentSelectionForm {
  patchValue(value: Partial<Record<AddResourceParentFormControlName, string | null>>): void;
}

const DEFAULT_PARENT_RESOURCE_DESCRIPTOR: AddResourceParentResourceDescriptor = {
  suffix: 'ASP',
  icon: 'dns',
  filterType: 'AppServicePlan',
  formControlName: 'appServicePlanId',
  requiresPlanSelection: false,
  requiresContainerRegistryLoading: false,
};

const PARENT_RESOURCE_DESCRIPTORS: Partial<Record<ResourceTypeEnum, AddResourceParentResourceDescriptor>> = {
  [ResourceTypeEnum.WebApp]: {
    ...DEFAULT_PARENT_RESOURCE_DESCRIPTOR,
    requiresPlanSelection: true,
    requiresContainerRegistryLoading: true,
  },
  [ResourceTypeEnum.FunctionApp]: {
    ...DEFAULT_PARENT_RESOURCE_DESCRIPTOR,
    requiresPlanSelection: true,
    requiresContainerRegistryLoading: true,
  },
  [ResourceTypeEnum.ContainerApp]: {
    suffix: 'CAE',
    icon: 'cloud_queue',
    filterType: 'ContainerAppEnvironment',
    formControlName: 'containerAppEnvironmentId',
    requiresPlanSelection: true,
    requiresContainerRegistryLoading: true,
  },
  [ResourceTypeEnum.ApplicationInsights]: {
    suffix: 'LAW',
    icon: 'analytics',
    filterType: 'LogAnalyticsWorkspace',
    formControlName: 'logAnalyticsWorkspaceId',
    requiresPlanSelection: true,
    requiresContainerRegistryLoading: false,
  },
  [ResourceTypeEnum.ContainerAppEnvironment]: {
    suffix: 'LAW',
    icon: 'analytics',
    filterType: 'LogAnalyticsWorkspace',
    formControlName: 'logAnalyticsWorkspaceId',
    requiresPlanSelection: true,
    requiresContainerRegistryLoading: false,
  },
  [ResourceTypeEnum.SqlDatabase]: {
    suffix: 'SQL',
    icon: 'dns',
    filterType: 'SqlServer',
    formControlName: 'sqlServerId',
    requiresPlanSelection: true,
    requiresContainerRegistryLoading: false,
  },
};

export function resolveAddResourceParentResourceDescriptor(type: ResourceTypeEnum | null): AddResourceParentResourceDescriptor {
  return type ? PARENT_RESOURCE_DESCRIPTORS[type] ?? DEFAULT_PARENT_RESOURCE_DESCRIPTOR : DEFAULT_PARENT_RESOURCE_DESCRIPTOR;
}

export function resolveAllowedChildResourceTypes(parentResourceType: string | undefined): ResourceTypeEnum[] | null {
  return (PARENT_CHILD_RESOURCE_TYPES[parentResourceType ?? ''] as ResourceTypeEnum[] | undefined) ?? null;
}

export function patchAddResourceParentResourceSelection(
  form: AddResourceParentSelectionForm,
  type: ResourceTypeEnum | null,
  planId: string | null,
): void {
  const descriptor = resolveAddResourceParentResourceDescriptor(type);
  form.patchValue({
    appServicePlanId: descriptor.formControlName === 'appServicePlanId' ? (planId ?? '') : '',
    containerAppEnvironmentId: descriptor.formControlName === 'containerAppEnvironmentId' ? (planId ?? '') : '',
    logAnalyticsWorkspaceId: descriptor.formControlName === 'logAnalyticsWorkspaceId' ? (planId ?? '') : '',
    sqlServerId: descriptor.formControlName === 'sqlServerId' ? (planId ?? '') : '',
  });
}
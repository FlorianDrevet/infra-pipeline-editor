import { computed, inject, signal, Signal } from '@angular/core';

import { NetworkingProfileService } from '../../../../shared/services/networking-profile.service';
import {
  NetworkingProfileResponse,
  SetNetworkingProfileRequest,
} from '../../../../shared/interfaces/networking-profile.interface';
import { AzureResourceResponse } from '../../../../shared/interfaces/resource-group.interface';
import { RESOURCE_TYPE_ICONS } from '../../../../shared/resource-metadata/resource-type.metadata';
import {
  NETWORKING_MODE_OPTIONS,
  VNET_SOURCE_TYPE_OPTIONS,
  DNS_MODE_OPTIONS,
} from './networking.constants';
import {
  ConfigDetailNetworkingSectionViewModel,
  NetworkingResourceItem,
} from './config-detail-networking-section.view-model';

export interface ConfigDetailNetworkingSectionController {
  readonly viewModel: Signal<ConfigDetailNetworkingSectionViewModel>;
  load(): Promise<void>;
  reset(): void;
}

interface ConfigDetailNetworkingSectionControllerDependencies {
  getConfigId(): string | null;
  getResources(): AzureResourceResponse[];
}

export function createConfigDetailNetworkingSectionController(
  dependencies: ConfigDetailNetworkingSectionControllerDependencies,
): ConfigDetailNetworkingSectionController {
  const networkingService = inject(NetworkingProfileService);

  const isLoading = signal(false);
  const isSaving = signal(false);
  const errorKey = signal('');
  const mode = signal('Simplified');
  const vnetSourceType = signal('CreateNew');
  const existingVnetResourceId = signal('');
  const createNewAddressSpace = signal('');
  const createNewSubnetAddressPrefix = signal('');
  const privateEndpointsSubnetName = signal('');
  const dnsMode = signal('AutoManaged');
  const dnsHubResourceGroupId = signal('');
  const dnsHubSubscriptionId = signal('');
  const privatizationState = signal<Record<string, boolean>>({});
  const privatizationSaving = signal<Record<string, boolean>>({});

  const showVnetPanel = computed(() => mode() !== 'Simplified');
  const showDnsPanel = computed(() => mode() !== 'Simplified');

  const resources = computed<NetworkingResourceItem[]>(() => {
    const allResources = dependencies.getResources();
    const state = privatizationState();
    const saving = privatizationSaving();
    return allResources.map((resource) => ({
      id: resource.id,
      name: resource.name,
      resourceType: resource.resourceType,
      icon: RESOURCE_TYPE_ICONS[resource.resourceType] || 'widgets',
      isPrivatized: state[resource.id] ?? false,
      saving: saving[resource.id] ?? false,
    }));
  });

  const applyProfile = (profile: NetworkingProfileResponse): void => {
    mode.set(profile.mode);
    vnetSourceType.set(profile.vnetSourceType);
    existingVnetResourceId.set(profile.existingVnetResourceId ?? '');
    createNewAddressSpace.set(profile.createNewAddressSpace ?? '');
    createNewSubnetAddressPrefix.set(profile.createNewSubnetAddressPrefix ?? '');
    privateEndpointsSubnetName.set(profile.privateEndpointsSubnetName ?? '');
    dnsMode.set(profile.dnsMode);
    dnsHubResourceGroupId.set(profile.dnsHubResourceGroupId ?? '');
    dnsHubSubscriptionId.set(profile.dnsHubSubscriptionId ?? '');
  };

  const load = async (): Promise<void> => {
    const configId = dependencies.getConfigId();
    if (!configId) return;

    isLoading.set(true);
    errorKey.set('');
    try {
      const profile = await networkingService.get(configId);
      applyProfile(profile);
    } catch {
      // Profile might not exist yet — use defaults
    } finally {
      isLoading.set(false);
    }
  };

  const save = async (): Promise<void> => {
    const configId = dependencies.getConfigId();
    if (!configId || isSaving()) return;

    isSaving.set(true);
    errorKey.set('');
    try {
      const request: SetNetworkingProfileRequest = {
        mode: mode(),
        vnetSourceType: vnetSourceType(),
        existingVnetResourceId: existingVnetResourceId() || null,
        createNewAddressSpace: createNewAddressSpace() || null,
        createNewSubnetAddressPrefix: createNewSubnetAddressPrefix() || null,
        privateEndpointsSubnetName: privateEndpointsSubnetName() || null,
        dnsMode: dnsMode(),
        dnsHubResourceGroupId: dnsHubResourceGroupId() || null,
        dnsHubSubscriptionId: dnsHubSubscriptionId() || null,
      };
      const profile = await networkingService.set(configId, request);
      applyProfile(profile);
    } catch {
      errorKey.set('CONFIG_DETAIL_NETWORKING.SAVE_ERROR');
    } finally {
      isSaving.set(false);
    }
  };

  const onTogglePrivatization = (resourceId: string, isPrivatized: boolean): void => {
    const configId = dependencies.getConfigId();
    if (!configId) return;

    privatizationSaving.update((current) => ({ ...current, [resourceId]: true }));
    privatizationState.update((current) => ({ ...current, [resourceId]: isPrivatized }));

    networkingService.togglePrivatization(configId, resourceId, isPrivatized)
      .catch(() => {
        privatizationState.update((current) => ({ ...current, [resourceId]: !isPrivatized }));
        errorKey.set('CONFIG_DETAIL_NETWORKING.SAVE_ERROR');
      })
      .finally(() => {
        privatizationSaving.update((current) => ({ ...current, [resourceId]: false }));
      });
  };

  const viewModel: ConfigDetailNetworkingSectionViewModel = {
    isLoading,
    isSaving,
    errorKey,
    mode,
    vnetSourceType,
    existingVnetResourceId,
    createNewAddressSpace,
    createNewSubnetAddressPrefix,
    privateEndpointsSubnetName,
    dnsMode,
    dnsHubResourceGroupId,
    dnsHubSubscriptionId,
    resources,
    showVnetPanel,
    showDnsPanel,
    modeOptions: NETWORKING_MODE_OPTIONS,
    vnetSourceTypeOptions: VNET_SOURCE_TYPE_OPTIONS,
    dnsModeOptions: DNS_MODE_OPTIONS,
    onModeChange: (value) => { if (value !== null) mode.set(String(value)); },
    onVnetSourceTypeChange: (value) => { if (value !== null) vnetSourceType.set(String(value)); },
    onExistingVnetResourceIdChange: (value) => existingVnetResourceId.set(value),
    onCreateNewAddressSpaceChange: (value) => createNewAddressSpace.set(value),
    onCreateNewSubnetAddressPrefixChange: (value) => createNewSubnetAddressPrefix.set(value),
    onPrivateEndpointsSubnetNameChange: (value) => privateEndpointsSubnetName.set(value),
    onDnsModeChange: (value) => { if (value !== null) dnsMode.set(String(value)); },
    onDnsHubResourceGroupIdChange: (value) => dnsHubResourceGroupId.set(value),
    onDnsHubSubscriptionIdChange: (value) => dnsHubSubscriptionId.set(value),
    onTogglePrivatization,
    save,
  };

  const reset = (): void => {
    isLoading.set(false);
    isSaving.set(false);
    errorKey.set('');
    mode.set('Simplified');
    vnetSourceType.set('CreateNew');
    existingVnetResourceId.set('');
    createNewAddressSpace.set('');
    createNewSubnetAddressPrefix.set('');
    privateEndpointsSubnetName.set('');
    dnsMode.set('AutoManaged');
    dnsHubResourceGroupId.set('');
    dnsHubSubscriptionId.set('');
    privatizationState.set({});
    privatizationSaving.set({});
  };

  return { viewModel: computed(() => viewModel), load, reset };
}

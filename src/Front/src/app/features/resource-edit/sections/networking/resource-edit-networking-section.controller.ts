import { computed, inject, signal } from '@angular/core';

import { DsSelectOption } from '../../../../shared/components/ds';
import { PrivateEndpointConfigResponse } from '../../../../shared/interfaces/private-endpoint-config.interface';
import { AzureResourceResponse } from '../../../../shared/interfaces/resource-group.interface';
import { PrivateEndpointService } from '../../../../shared/services/private-endpoint.service';
import { VirtualNetworkService } from '../../../../shared/services/virtual-network.service';
import { ResourceEditNetworkingSection } from './resource-edit-networking-section.interface';

interface ResourceEditNetworkingSectionControllerDependencies {
  getInfraConfigId(): string;
  getResourceId(): string;
  getIsPrivatized(): boolean;
  getAllResources(): AzureResourceResponse[];
}

export function createResourceEditNetworkingSectionController(
  dependencies: ResourceEditNetworkingSectionControllerDependencies,
): ResourceEditNetworkingSection {
  const peService = inject(PrivateEndpointService);
  const vnetService = inject(VirtualNetworkService);

  const isPrivatized = signal(false);
  const privateEndpointConfig = signal<PrivateEndpointConfigResponse | null>(null);
  const isLoading = signal(false);
  const isSaving = signal(false);
  const isLoadingSubnets = signal(false);
  const errorKey = signal('');
  const subnetOptions = signal<DsSelectOption[]>([]);

  const vnetOptions = computed<DsSelectOption[]>(() => {
    const resources = dependencies.getAllResources();
    return resources
      .filter((r) => r.resourceType === 'VirtualNetwork')
      .map((r) => ({ value: r.id, label: r.name }));
  });

  const load = (): void => {
    isPrivatized.set(dependencies.getIsPrivatized());
    const infraConfigId = dependencies.getInfraConfigId();
    const resourceId = dependencies.getResourceId();
    if (!infraConfigId || !resourceId || !dependencies.getIsPrivatized()) {
      return;
    }

    isLoading.set(true);
    peService.getPrivateEndpointConfig(infraConfigId, resourceId)
      .then((config) => privateEndpointConfig.set(config))
      .catch(() => errorKey.set('RESOURCE_EDIT.NETWORKING.LOAD_ERROR'))
      .finally(() => isLoading.set(false));
  };

  const togglePrivatization = async (value: boolean): Promise<void> => {
    const infraConfigId = dependencies.getInfraConfigId();
    const resourceId = dependencies.getResourceId();
    if (!infraConfigId || !resourceId) {
      return;
    }

    const previous = isPrivatized();
    isPrivatized.set(value);
    errorKey.set('');

    try {
      await peService.toggleResourcePrivatization(infraConfigId, resourceId, value);
      if (!value) {
        privateEndpointConfig.set(null);
      }
    } catch {
      isPrivatized.set(previous);
      errorKey.set('RESOURCE_EDIT.NETWORKING.TOGGLE_ERROR');
    }
  };

  const saveConfig = async (config: {
    virtualNetworkId: string;
    subnetName: string;
    dnsMode: string;
    dnsHubResourceGroupId?: string;
    dnsHubSubscriptionId?: string;
  }): Promise<void> => {
    const infraConfigId = dependencies.getInfraConfigId();
    const resourceId = dependencies.getResourceId();
    if (!infraConfigId || !resourceId) {
      return;
    }

    isSaving.set(true);
    errorKey.set('');

    try {
      await peService.setPrivateEndpointConfig(infraConfigId, resourceId, config);
      privateEndpointConfig.set({
        virtualNetworkId: config.virtualNetworkId,
        subnetName: config.subnetName,
        dnsMode: config.dnsMode,
        dnsHubResourceGroupId: config.dnsHubResourceGroupId ?? null,
        dnsHubSubscriptionId: config.dnsHubSubscriptionId ?? null,
      });
    } catch {
      errorKey.set('RESOURCE_EDIT.NETWORKING.SAVE_ERROR');
    } finally {
      isSaving.set(false);
    }
  };

  const removeConfig = async (): Promise<void> => {
    const infraConfigId = dependencies.getInfraConfigId();
    const resourceId = dependencies.getResourceId();
    if (!infraConfigId || !resourceId) {
      return;
    }

    isSaving.set(true);
    errorKey.set('');

    try {
      await peService.removePrivateEndpointConfig(infraConfigId, resourceId);
      privateEndpointConfig.set(null);
    } catch {
      errorKey.set('RESOURCE_EDIT.NETWORKING.REMOVE_ERROR');
    } finally {
      isSaving.set(false);
    }
  };

  const loadSubnetsForVnet = async (vnetId: string): Promise<void> => {
    if (!vnetId) {
      subnetOptions.set([]);
      return;
    }

    isLoadingSubnets.set(true);
    try {
      const vnet = await vnetService.getById(vnetId);
      subnetOptions.set(
        vnet.subnets.map((s) => ({ value: s.name, label: s.name }))
      );
    } catch {
      subnetOptions.set([]);
    } finally {
      isLoadingSubnets.set(false);
    }
  };

  return {
    isPrivatized,
    privateEndpointConfig,
    vnetOptions,
    subnetOptions,
    isLoadingSubnets,
    isLoading,
    isSaving,
    errorKey,
    load,
    loadSubnetsForVnet,
    togglePrivatization,
    saveConfig,
    removeConfig,
  };
}
